using QT.Core.Primitives;
using QT.Core.Quality;
using QT.Features.Engine;
using QT.Market.Events;
using QT.Runtime.AnalyticsRuntime;
using QT.Simulation.Replay;
using QT.Storage.Schemas;

namespace MBO_Market_Data_Analytics.Tests
{
    public static class RecorderReplayArchitectureTests
    {
        private static readonly DateTime T = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        public static void RunAll()
        {
            Console.WriteLine("Recorder / replay / architecture:");
            RecorderIncludesSchemaAndHash();
            UnavailableValuesNotSerializedAsZero();
            ReplayUsesSameRuntimePath();
            RuntimeHonorsFeatureCadence();
            FeatureProjectsDoNotReferenceQuantower();
            RuntimeDoesNotReferenceBrokerOrderMethods();
            QuantowerHostDoesNotCompileStrategy();
        }

        private static void RecorderIncludesSchemaAndHash()
        {
            TestHarness.Begin("recorder includes schema version and configuration hash");
            string dir = TestDir();
            var runtime = new AnalyticsRuntime(Config(dir));
            runtime.Start(T);
            runtime.BeginSnapshot(T, BookMode.Mbp);
            runtime.ApplySnapshotLevel(T, BookSide.Bid, 400, 10);
            runtime.ApplySnapshotLevel(T, BookSide.Ask, 401, 10);
            runtime.EndSnapshot(T, T);
            runtime.Stop();
            string featureFile = Path.Combine(dir, "feature_snapshots_v2.csv");
            string text = File.ReadAllText(featureFile);
            TestHarness.IsTrue(text.Contains("schema_version"), "header has schema version");
            TestHarness.IsTrue(text.Contains("config_hash"), "header has config hash");
            TestHarness.IsTrue(text.Contains(RecorderConfig.CurrentSchemaVersion.ToString()), "rows include current schema version");
        }

        private static void UnavailableValuesNotSerializedAsZero()
        {
            TestHarness.Begin("unavailable values are not serialized as numeric zero");
            string dir = TestDir();
            var runtime = new AnalyticsRuntime(Config(dir));
            runtime.Start(T);
            runtime.BeginSnapshot(T, BookMode.Mbp);
            runtime.ApplySnapshotLevel(T, BookSide.Bid, 400, 10);
            runtime.ApplySnapshotLevel(T, BookSide.Ask, 401, 10);
            runtime.EndSnapshot(T, T);
            runtime.Stop();
            var lines = File.ReadAllLines(Path.Combine(dir, "feature_snapshots_v2.csv"));
            var line = lines.FirstOrDefault(x => x.Contains("fp.confirmed"));
            TestHarness.IsTrue(line != null, "floating-pair unavailable line exists");
            TestHarness.IsTrue(line!.Contains("Unavailable"), "quality is Unavailable");
            TestHarness.IsTrue(!line.Contains("fp.confirmed,Confirmed Floating Pairs,0,"), "numeric field is empty, not zero");
        }

        private static void ReplayUsesSameRuntimePath()
        {
            TestHarness.Begin("replay/live parity for runtime features");
            var cfg = Config("");
            var events = new[]
            {
                NormalizedMarketEvent.BookLevel(1, T, T, "MNQ", BookSide.Bid, 400, 100, 10, null, false),
                NormalizedMarketEvent.BookLevel(2, T.AddMilliseconds(1), T.AddMilliseconds(1), "MNQ", BookSide.Ask, 401, 100.25, 10, null, false),
                NormalizedMarketEvent.Trade(3, T.AddMilliseconds(2), T.AddMilliseconds(2), "MNQ", 100.25, 2, TradeAggressor.Buy)
            };

            var live = new AnalyticsRuntime(cfg);
            live.Start(T);
            foreach (var evt in events) live.OnMarketEvent(evt);
            var liveSnapshot = live.Current.Features;
            live.Stop();

            var replay = new DeterministicReplayRunner(cfg).Run(events);
            var replaySnapshot = replay.FeatureSnapshots.Last();
            TestHarness.AreEqual(liveSnapshot.Book.SpreadTicks, replaySnapshot.Book.SpreadTicks, 0, "same spread");
            TestHarness.IsTrue(liveSnapshot.MarketState.Regime == replaySnapshot.MarketState.Regime, "same regime");
            TestHarness.IsTrue(liveSnapshot.FloatingPairs.Status == replaySnapshot.FloatingPairs.Status, "same pair status");
        }

        private static void RuntimeHonorsFeatureCadence()
        {
            TestHarness.Begin("runtime throttles feature publication during event bursts");
            var cfg = Config("", TimeSpan.FromSeconds(1));
            var runtime = new AnalyticsRuntime(cfg);
            runtime.Start(T);

            runtime.OnMarketEvent(NormalizedMarketEvent.BookLevel(1, T, T, "MNQ", BookSide.Bid, 400, 100, 10, null, false));
            runtime.OnMarketEvent(NormalizedMarketEvent.BookLevel(2, T.AddMilliseconds(1), T.AddMilliseconds(1), "MNQ", BookSide.Ask, 401, 100.25, 10, null, false));
            long validSequence = runtime.Current.Features.Sequence;

            for (int i = 0; i < 100; i++)
            {
                var t = T.AddMilliseconds(2 + i);
                runtime.OnMarketEvent(NormalizedMarketEvent.BookLevel(3 + i, t, t, "MNQ", BookSide.Bid, 400, 100, 10 + i, null, false));
            }

            TestHarness.AreEqual(validSequence, runtime.Current.Features.Sequence, 0, "burst inside cadence does not publish every event");
            runtime.AdvanceTime(T.AddSeconds(2));
            TestHarness.IsTrue(runtime.Current.Features.Sequence > validSequence, "cadence tick publishes latest book");
            runtime.Stop();
        }

        private static void FeatureProjectsDoNotReferenceQuantower()
        {
            TestHarness.Begin("feature projects do not reference Quantower");
            string text = ReadAll("src", "QT.Features");
            TestHarness.IsTrue(!text.Contains("TradingPlatform.BusinessLayer"), "no Quantower namespace in QT.Features");
        }

        private static void RuntimeDoesNotReferenceBrokerOrderMethods()
        {
            TestHarness.Begin("analytics runtime does not reference broker order methods");
            string text = ReadAll("src", "QT.Runtime");
            TestHarness.IsTrue(!text.Contains("PlaceOrder"), "no PlaceOrder");
            TestHarness.IsTrue(!text.Contains("CancelOrder"), "no CancelOrder");
            TestHarness.IsTrue(!text.Contains("ModifyOrder"), "no ModifyOrder");
        }

        private static void QuantowerHostDoesNotCompileStrategy()
        {
            TestHarness.Begin("Quantower host does not compile active strategy or monolithic calculator");
            string csproj = File.ReadAllText(Path.Combine(Root(), "DataAnalytics.csproj"));
            TestHarness.IsTrue(!csproj.Contains("DataAnalyticsStrategy.cs"), "strategy not compiled");
            TestHarness.IsTrue(!csproj.Contains("ShadowSimulator.cs"), "shadow simulator not compiled");
            TestHarness.IsTrue(!csproj.Contains("ReplayBacktester.cs"), "strategy replay not compiled");
            TestHarness.IsTrue(!csproj.Contains("DataAnalyticsCalculator.cs"), "monolithic calculator not compiled");
            TestHarness.IsTrue(!csproj.Contains("Broker.cs"), "broker adapter not compiled into host");
        }

        private static AnalyticsRuntimeConfig Config(string recorderPath, TimeSpan? featureCadence = null)
            => new()
            {
                Symbol = "MNQ",
                RuntimeSessionId = "test-session",
                SourceCommit = "test",
                BuildConfiguration = "Test",
                PreferredBookMode = BookMode.Mbp,
                FeatureCadence = featureCadence ?? TimeSpan.FromMilliseconds(250),
                Book = new QT.Market.Lifecycle.OrderBookEngineConfig
                {
                    Symbol = "MNQ",
                    TickSize = 0.25,
                    PreferredMode = BookMode.Mbp,
                    TopDepth = 10,
                    StaleTimeout = TimeSpan.FromSeconds(3)
                },
                Features = new FeatureEngineConfig(),
                Recorder = new RecorderConfig
                {
                    Enabled = !string.IsNullOrWhiteSpace(recorderPath),
                    OutputPath = recorderPath,
                    RawEvents = true,
                    FeatureSnapshots = true,
                    Transitions = true,
                    Diagnostics = true
                }
            };

        private static string TestDir()
        {
            string dir = Path.Combine(@"C:\tmp", "QT_API_V2_TEST_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return dir;
        }

        private static string ReadAll(params string[] parts)
        {
            string dir = Path.Combine(new[] { Root() }.Concat(parts).ToArray());
            return string.Join("\n", Directory.EnumerateFiles(dir, "*.cs", SearchOption.AllDirectories).Select(File.ReadAllText));
        }

        private static string Root()
        {
            var dir = AppContext.BaseDirectory;
            while (!File.Exists(Path.Combine(dir, "DataAnalytics.csproj")))
            {
                var parent = Directory.GetParent(dir);
                if (parent == null) throw new InvalidOperationException("repo root not found");
                dir = parent.FullName;
            }
            return dir;
        }
    }
}
