"""
Honest edge test for the strategy's signal (BuySellVolumeRatio60), as a forward-return
event study on the captured feature-store CSV. NOT a full strategy backtest (that conflates
the signal with the broken fill model + risk rules). We ask only: does the ratio predict the
next move, beyond drift and beyond costs?

Key controls (added in the extended version):
  * DETRENDED forward returns  = raw forward return minus the segment's mean forward return at
    that horizon. This removes the unconditional drift so a one-way market can't masquerade as
    signal (the confound the first pass exposed).
  * SPEARMAN (rank) IC as the primary measure  — robust to the fat tails of flow/returns.
  * IC split by REGIME (trend up/down, volatility low/high) to see if any edge is regime-specific.
  * Momentum vs mean-reversion read straight off the sign of the detrended IC.

Price is reconstructed from captured columns:  last = SessionVWAP + VWAPDistance*tick.
Signals are edge-triggered (threshold CROSSINGS), matching the strategy's one-shot latch.
"""
import csv, sys, math
from datetime import datetime

CSV = r"C:\Quantower\Settings\Scripts\ScriptsData\microstructure_features.csv"
TICK = 0.25
SEG_GAP_S = 5.0
HORIZONS_S = [0.5, 1.0, 2.0, 5.0, 10.0, 30.0, 60.0]
COST_TICKS = 2.0
TREND_WIN_S = 60.0          # trailing window for regime classification
TRIG_BUY, TRIG_SELL = 1.3, round(1.0 / 1.3, 4)   # the only threshold with usable trigger counts

def parse_ts(s):
    s = s.strip().rstrip("Z")
    if "." in s:
        head, frac = s.split("."); s = head + "." + (frac + "000000")[:6]
    return datetime.fromisoformat(s).timestamp()

def load():
    rows = []
    with open(CSV, newline="") as f:
        r = csv.reader(f); hdr = next(r)
        ix = {n: i for i, n in enumerate(hdr)}
        for n in ["Timestamp", "Symbol", "BuySellVolumeRatio60", "SessionVWAP", "VWAPDistance"]:
            if n not in ix: sys.exit(f"missing column: {n}")
        for row in r:
            try:
                t = parse_ts(row[ix["Timestamp"]]); sym = row[ix["Symbol"]]
                ratio = float(row[ix["BuySellVolumeRatio60"]])
                price = float(row[ix["SessionVWAP"]]) + float(row[ix["VWAPDistance"]]) * TICK
            except Exception:
                continue
            if price > 0 and ratio > 0:
                rows.append((t, sym, ratio, price))
    return rows

def segments(rows):
    by = {}
    for t, sym, ratio, price in rows: by.setdefault(sym, []).append((t, ratio, price))
    segs = []
    for sym, lst in by.items():
        lst.sort(key=lambda x: x[0]); cur = [lst[0]]
        for rec in lst[1:]:
            if rec[0] - cur[-1][0] > SEG_GAP_S: segs.append(cur); cur = [rec]
            else: cur.append(rec)
        segs.append(cur)
    return [s for s in segs if len(s) >= 20]

def fwd_returns(seg, horizon):
    """Two-pointer forward return in ticks for each i (None if not enough lookahead)."""
    n = len(seg); out = [None] * n; j = 0
    for i in range(n):
        if j < i + 1: j = i + 1
        tgt = seg[i][0] + horizon
        while j < n and seg[j][0] < tgt: j += 1
        if j < n: out[i] = (seg[j][2] - seg[i][2]) / TICK
    return out

def trailing_drift_vol(seg):
    """Trailing TREND_WIN_S return (ticks) and trailing volatility (std of 1-step ret ticks)."""
    n = len(seg); td = [None] * n; tv = [None] * n; k = 0
    for i in range(n):
        while seg[k][0] < seg[i][0] - TREND_WIN_S: k += 1
        if i - k >= 5:
            td[i] = (seg[i][2] - seg[k][2]) / TICK
            steps = [(seg[m][2] - seg[m-1][2]) / TICK for m in range(k + 1, i + 1)]
            mu = sum(steps) / len(steps)
            tv[i] = math.sqrt(sum((x - mu) ** 2 for x in steps) / len(steps))
    return td, tv

def rank(xs):
    order = sorted(range(len(xs)), key=lambda i: xs[i])
    rk = [0.0] * len(xs); i = 0
    while i < len(order):
        j = i
        while j + 1 < len(order) and xs[order[j+1]] == xs[order[i]]: j += 1
        avg = (i + j) / 2.0
        for k in range(i, j + 1): rk[order[k]] = avg
        i = j + 1
    return rk

def pearson(xs, ys):
    n = len(xs)
    if n < 5: return 0.0
    mx = sum(xs)/n; my = sum(ys)/n
    cov = sum((x-mx)*(y-my) for x, y in zip(xs, ys))
    vx = sum((x-mx)**2 for x in xs); vy = sum((y-my)**2 for y in ys)
    return cov/math.sqrt(vx*vy) if vx > 0 and vy > 0 else 0.0

def spearman(xs, ys):
    return pearson(rank(xs), rank(ys)) if len(xs) >= 5 else 0.0

def stats(xs):
    n = len(xs)
    if n == 0: return (0, 0.0, 0.0)
    m = sum(xs)/n
    sd = math.sqrt(sum((x-m)**2 for x in xs)/n) if n > 1 else 0.0
    t = (m/(sd/math.sqrt(n))) if sd > 0 else 0.0
    return (n, m, t)

def main():
    rows = load(); segs = segments(rows)
    syms = sorted({r[1] for r in rows})
    span = (max(r[0] for r in rows) - min(r[0] for r in rows)) / 60.0
    print(f"usable samples: {len(rows)}   symbols: {syms}   segments: {len(segs)}   calendar span: {span:.0f} min\n")

    # Per-sample arrays aggregated across segments.
    G = {h: {"ratio": [], "raw": [], "detr": [], "td": [], "tv": []} for h in HORIZONS_S}
    TRIG = {h: {"buy_raw": [], "buy_detr": [], "sell_raw": [], "sell_detr": []} for h in HORIZONS_S}

    for seg in segs:
        td, tv = trailing_drift_vol(seg)
        for h in HORIZONS_S:
            fr = fwd_returns(seg, h)
            valid = [x for x in fr if x is not None]
            base = sum(valid)/len(valid) if valid else 0.0     # segment drift at this horizon
            for i in range(len(seg)):
                if fr[i] is None: continue
                G[h]["ratio"].append(seg[i][1]); G[h]["raw"].append(fr[i])
                G[h]["detr"].append(fr[i] - base)
                G[h]["td"].append(td[i]); G[h]["tv"].append(tv[i])
                if i > 0:
                    prev, cur = seg[i-1][1], seg[i][1]
                    if cur >= TRIG_BUY and prev < TRIG_BUY:
                        TRIG[h]["buy_raw"].append(fr[i]); TRIG[h]["buy_detr"].append(fr[i]-base)
                    if cur <= TRIG_SELL and prev > TRIG_SELL:
                        TRIG[h]["sell_raw"].append(fr[i]); TRIG[h]["sell_detr"].append(fr[i]-base)

    print("=== INFORMATION COEFFICIENT (ratio vs forward return) ===")
    print("   Spearman is primary (robust). 'detr' = drift-removed. |IC|>~0.05 starts to matter.")
    for h in HORIZONS_S:
        g = G[h]; n = len(g["ratio"])
        ic_raw = spearman(g["ratio"], g["raw"]); ic_dt = spearman(g["ratio"], g["detr"])
        verdict = "momentum" if ic_dt > 0.02 else ("mean-rev" if ic_dt < -0.02 else "none")
        print(f"  H={h:>5.1f}s  n={n:>5}  IC_raw={ic_raw:+.3f}  IC_detr={ic_dt:+.3f}   -> {verdict}")
    print()

    print("=== IC (detrended, Spearman) BY REGIME ===")
    for h in (1.0, 5.0, 30.0):
        g = G[h]
        up = [i for i, d in enumerate(g["td"]) if d is not None and d > 0]
        dn = [i for i, d in enumerate(g["td"]) if d is not None and d < 0]
        tvv = [g["tv"][i] for i in range(len(g["tv"])) if g["tv"][i] is not None]
        vmed = sorted(tvv)[len(tvv)//2] if tvv else 0.0
        hi = [i for i in range(len(g["tv"])) if g["tv"][i] is not None and g["tv"][i] >= vmed]
        lo = [i for i in range(len(g["tv"])) if g["tv"][i] is not None and g["tv"][i] < vmed]
        def ic_sub(idx): return spearman([g["ratio"][i] for i in idx], [g["detr"][i] for i in idx])
        print(f"  H={h:>4.0f}s  trend-up n={len(up):>4} IC={ic_sub(up):+.3f} | "
              f"trend-dn n={len(dn):>4} IC={ic_sub(dn):+.3f} | "
              f"hi-vol n={len(hi):>4} IC={ic_sub(hi):+.3f} | lo-vol n={len(lo):>4} IC={ic_sub(lo):+.3f}")
    print()

    print(f"=== TRIGGER EDGE (crossings buy>= {TRIG_BUY} / sell<= {TRIG_SELL}), DETRENDED, ticks ===")
    print(f"   momentum edge = follow signal; mean-rev = fade it (= -momentum). net = momentum - {COST_TICKS:.0f}t cost.")
    for h in HORIZONS_S:
        t = TRIG[h]
        bn, bm, bt = stats(t["buy_detr"]); sn, sm, st = stats(t["sell_detr"])
        mom = [x for x in t["buy_detr"]] + [-x for x in t["sell_detr"]]
        mn, mmean, mt = stats(mom)
        print(f"  H={h:>5.1f}s | buy n={bn:>3} detr={bm:+5.2f}t (t={bt:+4.1f}) | "
              f"sell n={sn:>3} detr={sm:+5.2f}t (t={st:+4.1f}) | "
              f"momentum={mmean:+5.2f}t (t={mt:+4.1f}) net={mmean-COST_TICKS:+5.2f}t")

if __name__ == "__main__":
    main()
