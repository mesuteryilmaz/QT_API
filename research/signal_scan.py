"""
Signal scan: detrended rank-IC of EVERY captured flow/book metric vs forward returns.
Answers "does anything we measure predict the next move?" — not just the 60s ratio. Short-window
signals (Delta1s/Delta5s) are non-overlapping with the 30-60s horizons, so they disambiguate
whether the faint 60s-ratio IC is genuine flow predictivity or a slow-autocorrelated artifact.

Price = SessionVWAP + VWAPDistance*tick. Forward returns are detrended per segment (drift removed).
"""
import csv, sys, math
from datetime import datetime

CSV = r"C:\Quantower\Settings\Scripts\ScriptsData\microstructure_features.csv"
TICK = 0.25
SEG_GAP_S = 5.0
HORIZONS = [1.0, 5.0, 30.0, 60.0]

# Curated stationary flow/book signals (skip cumulative + price-level columns).
SIGNALS = ["BuySellVolumeRatio60", "BuySellCountRatio60",
           "Delta1s", "Delta5s", "Delta30s", "Delta60s", "DeltaVelocity",
           "QueueImbalance", "DOMImbalance3", "DOMImbalance5", "DOMImbalance10",
           "BookPressure", "CancelRatio", "CancelVolumeRatio",
           "AbsorptionBuy", "AbsorptionSell"]

def parse_ts(s):
    s = s.strip().rstrip("Z")
    if "." in s:
        h, fr = s.split("."); s = h + "." + (fr + "000000")[:6]
    return datetime.fromisoformat(s).timestamp()

def load():
    rows = []
    with open(CSV, newline="") as f:
        r = csv.reader(f); hdr = next(r)
        ix = {n: i for i, n in enumerate(hdr)}
        sigs = [s for s in SIGNALS if s in ix]
        for row in r:
            try:
                t = parse_ts(row[ix["Timestamp"]]); sym = row[ix["Symbol"]]
                price = float(row[ix["SessionVWAP"]]) + float(row[ix["VWAPDistance"]]) * TICK
                vals = {s: float(row[ix[s]]) for s in sigs}
            except Exception:
                continue
            if price > 0:
                rows.append((t, sym, price, vals))
    return rows, sigs

def segments(rows):
    by = {}
    for rec in rows: by.setdefault(rec[1], []).append(rec)
    segs = []
    for lst in by.values():
        lst.sort(key=lambda x: x[0]); cur = [lst[0]]
        for rec in lst[1:]:
            if rec[0] - cur[-1][0] > SEG_GAP_S: segs.append(cur); cur = [rec]
            else: cur.append(rec)
        segs.append(cur)
    return [s for s in segs if len(s) >= 30]

def fwd(seg, h):
    n = len(seg); out = [None]*n; j = 0
    for i in range(n):
        if j < i+1: j = i+1
        tgt = seg[i][0] + h
        while j < n and seg[j][0] < tgt: j += 1
        if j < n: out[i] = (seg[j][2] - seg[i][2]) / TICK
    return out

def rank(xs):
    order = sorted(range(len(xs)), key=lambda i: xs[i]); rk = [0.0]*len(xs); i = 0
    while i < len(order):
        j = i
        while j+1 < len(order) and xs[order[j+1]] == xs[order[i]]: j += 1
        for k in range(i, j+1): rk[order[k]] = (i+j)/2.0
        i = j+1
    return rk

def pearson(xs, ys):
    n = len(xs)
    if n < 30: return 0.0
    mx = sum(xs)/n; my = sum(ys)/n
    cov = sum((x-mx)*(y-my) for x, y in zip(xs, ys))
    vx = sum((x-mx)**2 for x in xs); vy = sum((y-my)**2 for y in ys)
    return cov/math.sqrt(vx*vy) if vx > 0 and vy > 0 else 0.0

def spearman(xs, ys): return pearson(rank(xs), rank(ys))

def main():
    rows, sigs = load(); segs = segments(rows)
    print(f"samples: {len(rows)}   segments: {len(segs)}   signals: {len(sigs)}")
    print("detrended rank-IC (Spearman) of each signal vs forward return. |IC|>~0.05 ~ worth a look.")
    print("note: Delta1s/Delta5s are short-window (non-overlapping with 30/60s horizons).\n")

    # Aggregate per horizon: signal values + detrended forward returns.
    data = {h: {"fwd": [], "sig": {s: [] for s in sigs}} for h in HORIZONS}
    for seg in segs:
        for h in HORIZONS:
            fr = fwd(seg, h)
            valid = [x for x in fr if x is not None]
            base = sum(valid)/len(valid) if valid else 0.0
            for i in range(len(seg)):
                if fr[i] is None: continue
                data[h]["fwd"].append(fr[i]-base)
                for s in sigs: data[h]["sig"][s].append(seg[i][3][s])

    hdr = "  signal".ljust(24) + "".join(f"H={h:>4.0f}s".rjust(10) for h in HORIZONS)
    print(hdr); print("  " + "-"*(len(hdr)-2))
    # rank signals by max |IC| across horizons
    ic_table = {}
    for s in sigs:
        ic_table[s] = {h: spearman(data[h]["sig"][s], data[h]["fwd"]) for h in HORIZONS}
    for s in sorted(sigs, key=lambda s: -max(abs(ic_table[s][h]) for h in HORIZONS)):
        line = f"  {s.ljust(22)}" + "".join(f"{ic_table[s][h]:+.3f}".rjust(10) for h in HORIZONS)
        print(line)

if __name__ == "__main__":
    main()
