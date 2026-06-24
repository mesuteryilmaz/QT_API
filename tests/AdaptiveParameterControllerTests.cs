using System;

namespace MBO_Market_Data_Analytics.Tests
{
    // Validates the self-tuning controller: warmup (IsReady), ATR seeding, and the H-04 fix —
    // a ratio saturated at the clamp ceiling is flagged extreme (StandAside) even though the
    // extreme percentile sits exactly on that ceiling, while a mid ratio within a spread is Normal.
    //
    // All observations use a constant price within a single wall-clock minute so the seeded ATR
    // does not decay (no 1-minute bar closes), keeping the volatility gate at ~1.0 and isolating
    // the extreme-ratio logic under test.
    public static class AdaptiveParameterControllerTests
    {
        private const double Tick = 0.25;
        private static readonly DateTime T = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        private static AdaptiveParameterController Build()
        {
            var cfg = new AdaptiveParameterConfig
            {
                SampleIntervalMs = 1,   // sample (almost) every observation
                MinSamples = 20,        // become ready quickly
                RecalcSeconds = 0.1,    // recompute often
                RatioClampMax = 10.0
            };
            return new AdaptiveParameterController(cfg, Tick);
        }

        private static void Feed(AdaptiveParameterController c, int n, double stepMs, double price,
                                 Func<int, double> ratioFn, DateTime start)
        {
            var t = start;
            for (int i = 0; i < n; i++)
            {
                c.Observe(price, true, ratioFn(i), true, t);
                t = t.AddMilliseconds(stepMs);
            }
        }

        public static void RunAll()
        {
            Console.WriteLine("AdaptiveParameterController:");
            NotReadyBeforeMinSamples();
            ReadyAfterMinSamplesWithAtr();
            ExtremeRatioSaturationTriggersStandAside_H04();
            MidRatioWithinSpreadIsNormal();
        }

        private static void NotReadyBeforeMinSamples()
        {
            TestHarness.Begin("Not ready before MinSamples");
            var c = Build();
            c.SeedAtr(2.0, 100.0);
            Feed(c, 15, 10, 100.0, _ => 5.0, T); // 15 < MinSamples(20), enough elapsed for a recalc
            TestHarness.IsTrue(!c.Current.IsReady, "IsReady is false below MinSamples");
        }

        private static void ReadyAfterMinSamplesWithAtr()
        {
            TestHarness.Begin("Ready after MinSamples; ATR reported in ticks");
            var c = Build();
            c.SeedAtr(2.0, 100.0); // 2.0 price / 0.25 tick = 8 ticks
            Feed(c, 60, 5, 100.0, i => 3.0 + (i % 5), T);
            var ap = c.Current;
            TestHarness.IsTrue(ap.IsReady, "IsReady is true once warm");
            TestHarness.IsTrue(ap.SampleCount >= 20, "sample count reached MinSamples");
            TestHarness.AreEqual(8.0, ap.AtrTicks, 1e-6, "ATR in ticks (2.0 / 0.25)");
            TestHarness.IsTrue(ap.TakeProfitTicks >= 2 && ap.TakeProfitTicks <= 200, "TP ticks within clamp");
            TestHarness.IsTrue(ap.StopLossTicks >= 2 && ap.StopLossTicks <= 200, "SL ticks within clamp");
            TestHarness.IsTrue(ap.BuyThreshold >= ap.SellThreshold, "buy threshold >= sell threshold");
        }

        private static void ExtremeRatioSaturationTriggersStandAside_H04()
        {
            TestHarness.Begin("H-04: ratio saturated at clamp ceiling => StandAside");
            var c = Build();
            c.SeedAtr(2.0, 100.0);
            // All samples saturate at RatioClampMax (10). The 97th percentile is therefore 10, so a
            // strict '>' test could never flag the saturated current ratio of 10. The boundary-inclusive
            // '>=' (plus explicit saturation handling) must classify this as extreme.
            Feed(c, 60, 5, 100.0, _ => 10.0, T);
            var ap = c.Current;
            TestHarness.IsTrue(ap.IsReady, "controller is ready");
            TestHarness.IsTrue(ap.VolatilityRatio < 1.5, "no volatility spike (gate not the cause)");
            TestHarness.IsTrue(ap.Regime == RegimeState.StandAside, "saturated ratio is flagged extreme");
        }

        private static void MidRatioWithinSpreadIsNormal()
        {
            TestHarness.Begin("Mid ratio within a spread => Normal");
            var c = Build();
            c.SeedAtr(2.0, 100.0);
            // A spread of ratios 0..10 so the 97th percentile is ~10, then settle at a mid ratio (5)
            // which is neither saturated nor beyond the extreme percentile.
            Feed(c, 100, 3, 100.0, i => (double)(i % 11), T);
            Feed(c, 6, 120, 100.0, _ => 5.0, T.AddMilliseconds(310)); // settle + force a recalc at ratio 5
            var ap = c.Current;
            TestHarness.IsTrue(ap.IsReady, "controller is ready");
            TestHarness.IsTrue(ap.VolatilityRatio < 1.5, "no volatility spike");
            TestHarness.IsTrue(ap.Regime == RegimeState.Normal, "mid ratio within spread is Normal");
        }
    }
}
