using System.Collections.Concurrent;
using QT.Core.Primitives;
using QT.Features.AggregateLattice;
using QT.Features.Engine;
using QT.Features.FloatingPairs;
using QT.Features.MarketState;
using QT.Market.Events;
using QT.Runtime.AnalyticsRuntime;
using QT.Storage.Schemas;
using TradingPlatform.BusinessLayer;
using TradingPlatform.BusinessLayer.Integration;

namespace MBO_Market_Data_Analytics;

public sealed class AnalyticsEngineConfig
{
    public int SnapshotDepth = 360;
    public int UpdateFrequencyMs = 250;
    public bool PreferMbo = true;
    public int StaleTimeoutMs = 3000;
    public int QueueLimit = 10000;
    public bool EnableRecorder = false;
    public string RecorderPath = @"C:\Quantower\Settings\Scripts\ScriptsData\QT_API_V2";
    public bool EnableAggregateLattice = false;
    public long FloatingPairLargeThreshold = 20;
    public long FloatingPairVeryLargeThreshold = 200;
    public long FloatingPairMaxOffsetTicks = 250;
}

public sealed class AnalyticsEngineHost
{
    private readonly Symbol symbol;
    private readonly AnalyticsEngineConfig config;
    private readonly Action<string, LoggingLevel> log;
    private readonly double tickSize;

    private AnalyticsRuntime? runtime;
    private BlockingCollection<NormalizedMarketEvent>? queue;
    private CancellationTokenSource? cts;
    private Thread? worker;
    private long sequence;
    private bool initialized;
    private long queueOverflowCount;
    private int mboCoverageEvents;
    private int mboRealIdEvents;
    private bool mboCoveragePending;

    public AnalyticsEngineHost(Symbol symbol, AnalyticsEngineConfig config, Action<string, LoggingLevel> log)
    {
        this.symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
        this.config = config ?? throw new ArgumentNullException(nameof(config));
        this.log = log ?? ((_, _) => { });
        tickSize = symbol.TickSize > 0 ? symbol.TickSize : 1.0;
    }

    public AnalyticsRuntimeSnapshot? CurrentSnapshot => runtime?.Current;
    public bool IsInitialized => initialized;
    public long QueueOverflowCount => Interlocked.Read(ref queueOverflowCount);

    public void Start()
    {
        var now = Core.Instance.TimeUtils.DateTimeUtcNow;
        runtime = new AnalyticsRuntime(BuildRuntimeConfig());
        runtime.Start(now);

        queue = new BlockingCollection<NormalizedMarketEvent>(Math.Max(1000, config.QueueLimit));
        cts = new CancellationTokenSource();
        mboCoverageEvents = 0;
        mboRealIdEvents = 0;
        mboCoveragePending = config.PreferMbo;
        symbol.NewLast += OnNewLast;
        symbol.NewLevel2 += OnNewLevel2;
        initialized = true;

        SeedInitialSnapshot();

        worker = new Thread(ProcessLoop)
        {
            IsBackground = true,
            Name = "QT_API_V2_AnalyticsRuntime",
            Priority = ThreadPriority.AboveNormal
        };
        worker.Start();
        log("[QT_API V2] Analytics-only runtime started. No strategy or live-order path is active.", LoggingLevel.System);
    }

    public void Stop()
    {
        initialized = false;
        symbol.NewLast -= OnNewLast;
        symbol.NewLevel2 -= OnNewLevel2;
        cts?.Cancel();
        queue?.CompleteAdding();
        if (worker?.IsAlive == true)
            worker.Join(1000);
        runtime?.Stop();
        runtime = null;
        queue?.Dispose();
        queue = null;
        cts?.Dispose();
        cts = null;
    }

    private AnalyticsRuntimeConfig BuildRuntimeConfig()
    {
        var mode = config.PreferMbo ? BookMode.Mbo : BookMode.Mbp;
        return new AnalyticsRuntimeConfig
        {
            Symbol = symbol.Name,
            RuntimeSessionId = Guid.NewGuid().ToString("N"),
            SourceCommit = BuildInfo.SourceCommit,
            BuildConfiguration = "Quantower",
            PreferredBookMode = mode,
            FeatureCadence = TimeSpan.FromMilliseconds(Math.Max(50, config.UpdateFrequencyMs)),
            Book = new QT.Market.Lifecycle.OrderBookEngineConfig
            {
                Symbol = symbol.Name,
                TickSize = tickSize,
                PreferredMode = mode,
                TopDepth = Math.Max(5, Math.Min(50, config.SnapshotDepth)),
                StaleTimeout = TimeSpan.FromMilliseconds(Math.Max(250, config.StaleTimeoutMs)),
                BufferLimit = Math.Max(1000, config.QueueLimit)
            },
            Features = new FeatureEngineConfig
            {
                MarketState = new MarketStateConfig(),
                FloatingPairs = new FloatingPairConfig
                {
                    LargeThreshold = config.FloatingPairLargeThreshold,
                    VeryLargeThreshold = config.FloatingPairVeryLargeThreshold,
                    MaximumOffsetTicks = config.FloatingPairMaxOffsetTicks
                },
                AggregateLattice = new SymmetricAggregateLatticeConfig { Enabled = config.EnableAggregateLattice }
            },
            Recorder = new RecorderConfig
            {
                Enabled = config.EnableRecorder,
                OutputPath = config.RecorderPath,
                RawEvents = true,
                FeatureSnapshots = true,
                Transitions = true,
                Diagnostics = true
            }
        };
    }

    private void OnNewLast(Symbol sym, Last last)
    {
        if (!initialized || queue == null || last == null) return;
        var eventTime = last.Time != default ? last.Time : Core.Instance.TimeUtils.DateTimeUtcNow;
        var evt = NormalizedMarketEvent.Trade(
            Interlocked.Increment(ref sequence),
            eventTime,
            Core.Instance.TimeUtils.DateTimeUtcNow,
            symbol.Name,
            last.Price,
            ToQuantity(last.Size),
            ToAggressor(last.AggressorFlag));
        Enqueue(evt);
    }

    private void OnNewLevel2(Symbol sym, Level2Quote level2, DOMQuote dom)
    {
        if (!initialized || queue == null || level2 == null) return;
        var receive = Core.Instance.TimeUtils.DateTimeUtcNow;
        string? id = level2.Id;
        if (string.Equals(id, "FLUSH", StringComparison.OrdinalIgnoreCase))
        {
            Enqueue(NormalizedMarketEvent.Flush(Interlocked.Increment(ref sequence), receive, receive, symbol.Name, "FLUSH"));
            return;
        }

        var side = level2.PriceType == QuotePriceType.Bid ? BookSide.Bid : BookSide.Ask;
        long priceTicks = ToTicks(level2.Price);
        var evt = NormalizedMarketEvent.BookLevel(
            Interlocked.Increment(ref sequence),
            receive,
            receive,
            symbol.Name,
            side,
            priceTicks,
            level2.Price,
            ToQuantity(level2.Size),
            id,
            level2.Closed,
            level2.Priority,
            level2.NumberOrders);
        Enqueue(evt);
    }

    private void Enqueue(in NormalizedMarketEvent evt)
    {
        if (queue == null) return;
        if (!queue.TryAdd(evt))
        {
            Interlocked.Increment(ref queueOverflowCount);
            runtime?.MarkBufferOverflow(evt.ReceiveTimeUtc, "Quantower adapter queue overflow");
            while (queue.TryTake(out _)) { }
            SeedInitialSnapshot();
        }
    }

    private void ProcessLoop()
    {
        if (queue == null || cts == null || runtime == null) return;
        var token = cts.Token;
        long lastPublishTicks = DateTime.UtcNow.Ticks;
        long publishIntervalTicks = TimeSpan.FromMilliseconds(Math.Max(50, config.UpdateFrequencyMs)).Ticks;

        try
        {
            while (!token.IsCancellationRequested && !queue.IsCompleted)
            {
                if (queue.TryTake(out var evt, 10, token))
                {
                    try
                    {
                        runtime.OnMarketEvent(evt);
                        TrackMboCoverage(evt);
                        long nowTicks = DateTime.UtcNow.Ticks;
                        if (nowTicks - lastPublishTicks >= publishIntervalTicks)
                        {
                            runtime.AdvanceTime(DateTime.UtcNow);
                            lastPublishTicks = nowTicks;
                        }
                    }
                    catch (Exception ex)
                    {
                        log("[QT_API V2] Analytics event-processing error: " + ex, LoggingLevel.Error);
                    }
                }
                else
                {
                    long nowTicks = DateTime.UtcNow.Ticks;
                    if (nowTicks - lastPublishTicks >= publishIntervalTicks)
                    {
                        try
                        {
                            runtime.AdvanceTime(DateTime.UtcNow);
                            lastPublishTicks = nowTicks;
                        }
                        catch (Exception ex)
                        {
                            log("[QT_API V2] Analytics time-advance error: " + ex, LoggingLevel.Error);
                        }
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            log("[QT_API V2] Analytics worker error: " + ex, LoggingLevel.Error);
        }
    }

    private void TrackMboCoverage(NormalizedMarketEvent evt)
    {
        if (!mboCoveragePending || runtime == null || evt.Kind != MarketEventKind.BookLevel)
            return;

        mboCoverageEvents++;
        if (!string.IsNullOrWhiteSpace(evt.OrderId))
            mboRealIdEvents++;

        if (mboCoverageEvents == 100)
        {
            double coverage = mboRealIdEvents / 100.0;
            if (coverage < 0.5)
            {
                log($"[QT_API V2] MBO identity coverage too low ({coverage:P0}); downgrading to MBP with a fresh epoch.", LoggingLevel.System);
                runtime.DowngradeToMbp(DateTime.UtcNow, "MBO identity coverage below threshold");
                mboCoveragePending = false;
                SeedMbpSnapshot();
            }
            else
            {
                log($"[QT_API V2] MBO identity coverage confirmed ({coverage:P0}).", LoggingLevel.System);
                mboCoveragePending = false;
            }
        }
    }

    private void SeedInitialSnapshot()
    {
        if (runtime == null) return;
        if (config.PreferMbo && TrySeedMboSnapshot())
            return;
        if (!config.PreferMbo)
            SeedMbpSnapshot();
        else
            log("[QT_API V2] MBO snapshot unavailable or missing order IDs; waiting for live MBO events instead of mixing MBP seed with MBO increments.", LoggingLevel.System);
    }

    private bool TrySeedMboSnapshot()
    {
        if (runtime == null) return false;
        try
        {
            var snap = symbol.DepthOfMarket.GetDepthOfMarketAggregatedCollections(new GetLevel2ItemsParameters
            {
                AggregateMethod = AggregateMethod.None,
                GetMBOItems = true,
                LevelsCount = config.SnapshotDepth
            });
            if (snap == null) return false;

            var bids = snap.Bids ?? Array.Empty<Level2Item>();
            var asks = snap.Asks ?? Array.Empty<Level2Item>();
            int realIds = bids.Concat(asks).Count(x => x != null && !string.IsNullOrWhiteSpace(x.Id));
            if (realIds == 0) return false;

            DateTime now = Core.Instance.TimeUtils.DateTimeUtcNow;
            runtime.BeginSnapshot(now, BookMode.Mbo);
            foreach (var bid in bids)
                if (bid != null && !string.IsNullOrWhiteSpace(bid.Id))
                    runtime.ApplySnapshotLevel(now, BookSide.Bid, ToTicks(bid.Price), ToQuantity(bid.Size), bid.Id, bid.Priority, bid.NumberOrders);
            foreach (var ask in asks)
                if (ask != null && !string.IsNullOrWhiteSpace(ask.Id))
                    runtime.ApplySnapshotLevel(now, BookSide.Ask, ToTicks(ask.Price), ToQuantity(ask.Size), ask.Id, ask.Priority, ask.NumberOrders);
            runtime.EndSnapshot(now, now);
            log($"[QT_API V2] MBO snapshot seeded with {realIds} real-ID orders.", LoggingLevel.System);
            return true;
        }
        catch (Exception ex)
        {
            log("[QT_API V2] MBO snapshot seed failed: " + ex.Message, LoggingLevel.Error);
            return false;
        }
    }

    private void SeedMbpSnapshot()
    {
        if (runtime == null) return;
        try
        {
            var snap = symbol.DepthOfMarket.GetDepthOfMarketAggregatedCollections(new GetLevel2ItemsParameters
            {
                AggregateMethod = AggregateMethod.ByPriceLVL,
                LevelsCount = config.SnapshotDepth
            });
            if (snap == null) return;

            DateTime now = Core.Instance.TimeUtils.DateTimeUtcNow;
            runtime.BeginSnapshot(now, BookMode.Mbp);
            if (snap.Bids != null)
                foreach (var bid in snap.Bids)
                    if (bid != null)
                        runtime.ApplySnapshotLevel(now, BookSide.Bid, ToTicks(bid.Price), ToQuantity(bid.Size), null, bid.Priority, bid.NumberOrders);
            if (snap.Asks != null)
                foreach (var ask in snap.Asks)
                    if (ask != null)
                        runtime.ApplySnapshotLevel(now, BookSide.Ask, ToTicks(ask.Price), ToQuantity(ask.Size), null, ask.Priority, ask.NumberOrders);
            runtime.EndSnapshot(now, now);
            log("[QT_API V2] MBP snapshot seeded.", LoggingLevel.System);
        }
        catch (Exception ex)
        {
            log("[QT_API V2] MBP snapshot seed failed: " + ex.Message, LoggingLevel.Error);
        }
    }

    private long ToTicks(double price) => (long)Math.Round(price / tickSize);
    private static long ToQuantity(double size) => Math.Max(0, (long)Math.Round(size));

    private static TradeAggressor ToAggressor(AggressorFlag flag)
        => flag == AggressorFlag.Buy ? TradeAggressor.Buy
            : flag == AggressorFlag.Sell ? TradeAggressor.Sell
            : TradeAggressor.Unknown;
}
