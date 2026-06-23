using System;

namespace MBO_Market_Data_Analytics
{
    /// <summary>
    /// Selects between fixed (operator-supplied) trading parameters and the self-tuning adaptive
    /// controller.
    /// </summary>
    public enum StrategyParameterMode
    {
        Static,
        Adaptive
    }

    /// <summary>
    /// Tuning knobs for <see cref="AdaptiveParameterController"/>. Only a few are surfaced as
    /// Quantower inputs; the rest use sensible defaults.
    /// </summary>
    public sealed class AdaptiveParameterConfig
    {
        // Entry thresholds are the upper/lower percentiles of the recent signal-ratio distribution.
        public double EntryUpperPercentile = 0.85;
        public double EntryLowerPercentile = 0.15;

        // Ratio sampling / distribution window.
        public int SampleWindow = 5000;        // max retained ratio samples
        public int SampleIntervalMs = 250;     // min spacing between stored samples
        public int MinSamples = 480;           // ~2 min at 250 ms before the controller is "ready"
        public double RatioClampMax = 10.0;    // clamp ratio samples (saturation 999 -> 10)
        public double RecalcSeconds = 2.0;     // how often thresholds/ATR are recomputed

        // ATR-scaled brackets.
        public int AtrPeriod = 14;             // minutes (Wilder smoothing)
        public double TpAtrMultiplier = 1.0;
        public double SlAtrMultiplier = 1.0;
        public int MinBracketTicks = 2;
        public int MaxBracketTicks = 200;
    }

    /// <summary>
    /// Immutable snapshot of the currently effective adaptive parameters.
    /// </summary>
    public sealed class AdaptiveParameters
    {
        public bool IsReady { get; init; }
        public double BuyThreshold { get; init; }
        public double SellThreshold { get; init; }
        public double BuyResetThreshold { get; init; }
        public double SellResetThreshold { get; init; }
        public int TakeProfitTicks { get; init; }
        public int StopLossTicks { get; init; }
        public double AtrTicks { get; init; }
        public int SampleCount { get; init; }
    }

    /// <summary>
    /// Phase 1 of the regime-aware roadmap: self-tuning trading parameters.
    ///
    /// - Entry thresholds are derived from rolling **percentiles** of the live buyer/seller volume
    ///   ratio rather than fixed magic numbers, so they adapt per instrument and intraday. The
    ///   hysteresis reset level is the running median (re-arm when flow returns to typical).
    /// - Take-profit / stop-loss distances are scaled from a live **ATR** (seeded from history,
    ///   refined with live 1-minute bars), so risk is consistent across volatility regimes.
    /// - The controller reports <see cref="AdaptiveParameters.IsReady"/> = false until it has both an
    ///   ATR estimate and enough ratio samples, which the strategy uses to stand aside during warmup.
    ///
    /// Polarity (momentum vs mean-reversion) is intentionally NOT decided here — that arrives with the
    /// regime classifier in a later phase. Phase 1 keeps the existing momentum polarity.
    ///
    /// Thread-safe: <see cref="Observe"/> (worker thread) and <see cref="Current"/> (worker +
    /// order-update threads) are guarded by a single lock.
    /// </summary>
    public sealed class AdaptiveParameterController
    {
        private readonly AdaptiveParameterConfig cfg;
        private readonly double tickSize;
        private readonly object sync = new object();

        // Ratio sample ring buffer
        private readonly double[] samples;
        private int sampleHead;
        private int sampleCount;
        private long lastSampleTicks;
        private readonly long sampleIntervalTicks;

        // ATR (price units) via live 1-minute bars
        private bool atrSeeded;
        private double atr;
        private long curMinuteTicks = long.MinValue;
        private double curHigh, curLow, curClose;
        private double prevClose = double.NaN;

        private long lastRecalcTicks;
        private readonly long recalcIntervalTicks;

        private AdaptiveParameters current;

        public AdaptiveParameterController(AdaptiveParameterConfig config, double tickSize)
        {
            this.cfg = config ?? throw new ArgumentNullException(nameof(config));
            this.tickSize = tickSize > 0 ? tickSize : 1.0;
            this.samples = new double[Math.Max(16, cfg.SampleWindow)];
            this.sampleIntervalTicks = Math.Max(1, cfg.SampleIntervalMs) * TimeSpan.TicksPerMillisecond;
            this.recalcIntervalTicks = (long)(Math.Max(0.1, cfg.RecalcSeconds) * TimeSpan.TicksPerSecond);
            this.current = new AdaptiveParameters { IsReady = false };
        }

        /// <summary>Seeds the ATR from historical bars before live data arrives.</summary>
        public void SeedAtr(double atrPrice, double lastClose)
        {
            lock (sync)
            {
                if (atrPrice > 0)
                {
                    atr = atrPrice;
                    atrSeeded = true;
                }
                if (lastClose > 0)
                    prevClose = lastClose;
            }
        }

        public AdaptiveParameters Current
        {
            get { lock (sync) { return current; } }
        }

        /// <summary>
        /// Feeds one observation. Call every event; ratio sampling and recompute are throttled
        /// internally. <paramref name="ratioUsable"/> gates whether the ratio is sampled.
        /// </summary>
        public void Observe(double price, bool priceValid, double ratio, bool ratioUsable, DateTime nowUtc)
        {
            long now = nowUtc.Ticks;
            lock (sync)
            {
                if (priceValid)
                    UpdateBar(price, now);

                if (ratioUsable && (now - lastSampleTicks) >= sampleIntervalTicks)
                {
                    AddSample(Math.Clamp(ratio, 0.0, cfg.RatioClampMax));
                    lastSampleTicks = now;
                }

                if (now - lastRecalcTicks >= recalcIntervalTicks)
                    Recalc(now);
            }
        }

        private void AddSample(double v)
        {
            samples[sampleHead] = v;
            sampleHead = (sampleHead + 1) % samples.Length;
            if (sampleCount < samples.Length)
                sampleCount++;
        }

        private void UpdateBar(double price, long ticks)
        {
            long minute = (ticks / TimeSpan.TicksPerMinute) * TimeSpan.TicksPerMinute;

            if (curMinuteTicks == long.MinValue)
            {
                curMinuteTicks = minute;
                curHigh = curLow = curClose = price;
                return;
            }

            if (minute == curMinuteTicks)
            {
                if (price > curHigh) curHigh = price;
                if (price < curLow) curLow = price;
                curClose = price;
                return;
            }

            // Close the prior minute and fold its true range into the ATR (Wilder smoothing).
            double tr = curHigh - curLow;
            if (!double.IsNaN(prevClose))
                tr = Math.Max(tr, Math.Max(Math.Abs(curHigh - prevClose), Math.Abs(curLow - prevClose)));

            if (atrSeeded)
                atr += (tr - atr) / Math.Max(1, cfg.AtrPeriod);
            else
            {
                atr = tr;
                atrSeeded = true;
            }

            prevClose = curClose;
            curMinuteTicks = minute;
            curHigh = curLow = curClose = price;
        }

        private void Recalc(long nowTicks)
        {
            lastRecalcTicks = nowTicks;

            if (sampleCount < cfg.MinSamples || !atrSeeded)
            {
                current = new AdaptiveParameters
                {
                    IsReady = false,
                    SampleCount = sampleCount,
                    AtrTicks = atrSeeded ? atr / tickSize : 0.0
                };
                return;
            }

            var arr = new double[sampleCount];
            Array.Copy(samples, arr, sampleCount);
            Array.Sort(arr);

            double buyTh = Percentile(arr, cfg.EntryUpperPercentile);
            double sellTh = Percentile(arr, cfg.EntryLowerPercentile);
            double median = Percentile(arr, 0.5);

            double atrTicks = atr / tickSize;
            int tp = ClampTicks((int)Math.Round(atrTicks * cfg.TpAtrMultiplier));
            int sl = ClampTicks((int)Math.Round(atrTicks * cfg.SlAtrMultiplier));

            current = new AdaptiveParameters
            {
                IsReady = true,
                BuyThreshold = buyTh,
                SellThreshold = sellTh,
                BuyResetThreshold = median,
                SellResetThreshold = median,
                TakeProfitTicks = tp,
                StopLossTicks = sl,
                AtrTicks = atrTicks,
                SampleCount = sampleCount
            };
        }

        private int ClampTicks(int v) => Math.Clamp(v, cfg.MinBracketTicks, cfg.MaxBracketTicks);

        private static double Percentile(double[] sorted, double p)
        {
            if (sorted.Length == 0) return 0.0;
            int idx = (int)Math.Round(p * (sorted.Length - 1));
            idx = Math.Clamp(idx, 0, sorted.Length - 1);
            return sorted[idx];
        }
    }
}
