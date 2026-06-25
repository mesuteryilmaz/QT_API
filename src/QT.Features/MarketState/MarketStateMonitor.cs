using QT.Core.Primitives;
using QT.Core.Quality;
using QT.Market.Events;
using QT.Market.Snapshots;

namespace QT.Features.MarketState;

public sealed class MarketStateMonitor
{
    private readonly MarketStateConfig cfg;
    private readonly Queue<SpreadSample> spreadSamples = new();
    private readonly Queue<MidSample> midSamples = new();
    private readonly Queue<EventSample> bookEvents = new();
    private readonly Queue<EventSample> tradeEvents = new();
    private readonly Queue<EventSample> bboEvents = new();
    private readonly List<MarketStateTransition> transitions = new();

    private long epoch = -1;
    private DateTime epochStartUtc;
    private BookSnapshot? previousBook;
    private BookEventStats previousStats;
    private double touchDepthBaseline;
    private double top5DepthBaseline;
    private double eventRateBaseline;
    private double rvBaseline;
    private DateTime lastSampleUtc;
    private long previousSpread = -1;

    private bool episodeActive;
    private DateTime episodeStartedUtc;
    private long episodeStartSpread;
    private long episodePeakSpread;
    private TimeSpan lastRecoveryDuration;

    private MarketRegime currentRegime = MarketRegime.Unknown;
    private MarketRegime previousRegime = MarketRegime.Unknown;
    private MarketRegime candidateRegime = MarketRegime.Unknown;
    private DateTime candidateSinceUtc;
    private DateTime lastTransitionUtc;
    private string lastTransitionReason = "not initialized";

    public MarketStateMonitor(MarketStateConfig? config = null)
    {
        cfg = config ?? new MarketStateConfig();
        cfg.Validate();
    }

    public MarketStateSnapshot Current { get; private set; } =
        MarketStateSnapshot.Empty(DateTime.MinValue, 0, MetricQuality.Unavailable, "not initialized");

    public IReadOnlyList<MarketStateTransition> DrainTransitions()
    {
        if (transitions.Count == 0) return Array.Empty<MarketStateTransition>();
        var copy = transitions.ToArray();
        transitions.Clear();
        return copy;
    }

    public void Reset(long newEpoch, DateTime eventTimeUtc)
    {
        epoch = newEpoch;
        epochStartUtc = eventTimeUtc;
        previousBook = null;
        previousStats = default;
        touchDepthBaseline = 0;
        top5DepthBaseline = 0;
        eventRateBaseline = 0;
        rvBaseline = 0;
        lastSampleUtc = eventTimeUtc;
        previousSpread = -1;
        episodeActive = false;
        lastRecoveryDuration = TimeSpan.Zero;
        currentRegime = MarketRegime.Unknown;
        previousRegime = MarketRegime.Unknown;
        candidateRegime = MarketRegime.Unknown;
        candidateSinceUtc = eventTimeUtc;
        lastTransitionUtc = eventTimeUtc;
        lastTransitionReason = "epoch reset";
        spreadSamples.Clear();
        midSamples.Clear();
        bookEvents.Clear();
        tradeEvents.Clear();
        bboEvents.Clear();
        transitions.Clear();
        Current = MarketStateSnapshot.Empty(eventTimeUtc, newEpoch, MetricQuality.WarmingUp, "epoch reset");
    }

    public void OnMarketEvent(in NormalizedMarketEvent evt)
    {
        if (evt.Kind == MarketEventKind.Trade)
        {
            tradeEvents.Enqueue(new EventSample(evt.EventTimeUtc, Math.Max(0, evt.Quantity), 1));
            TrimEventQueue(tradeEvents, evt.EventTimeUtc - cfg.ActivityWindow);
        }
    }

    public MarketStateSnapshot OnBookSnapshot(BookSnapshot book, DateTime nowUtc)
    {
        if (book.BookEpoch != epoch)
            Reset(book.BookEpoch, book.EventTimeUtc);

        if (!book.HasTwoSidedBook || book.DataQuality == MetricQuality.Invalid || book.IsCrossed)
        {
            Current = MarketStateSnapshot.Empty(book.EventTimeUtc, book.BookEpoch, MetricQuality.Invalid,
                book.InvalidReason ?? "book invalid");
            currentRegime = MarketRegime.Unknown;
            previousBook = book;
            return Current;
        }

        if (book.DataQuality == MetricQuality.Stale)
        {
            Current = MarketStateSnapshot.Empty(book.EventTimeUtc, book.BookEpoch, MetricQuality.Stale, "feed stale");
            currentRegime = MarketRegime.Unknown;
            previousBook = book;
            return Current;
        }

        DateTime t = book.EventTimeUtc;
        long spread = book.SpreadTicks;
        spreadSamples.Enqueue(new SpreadSample(t, spread));
        TrimSpreadQueue(t - cfg.SpreadDistributionWindow);

        double mid2 = book.BestBidTicks!.Value + book.BestAskTicks!.Value;
        midSamples.Enqueue(new MidSample(t, mid2));
        TrimMidQueue(t - cfg.VolLongWindow);

        TrackBookEventDeltas(book, t);
        TrackBboChanges(book, t);
        UpdateSpreadEpisode(spread, t);

        double meanSpread30 = TimeWeightedMeanSpread(t, cfg.SpreadMeanWindow);
        long maxSpread60 = MaxSpread(t, cfg.SpreadDistributionWindow);
        double p90Spread60 = PercentileSpread(t, cfg.SpreadDistributionWindow, 0.90);
        double oneTickFraction = TimeAtOneTick(t, cfg.SpreadDistributionWindow);
        double widenRate = TransitionRate(t, widened: true);
        double narrowRate = TransitionRate(t, widened: false);

        double bookRate = Rate(bookEvents, t, cfg.ActivityWindow);
        double bookBurst = BurstRatio(bookEvents, t);
        double tradeRate = Rate(tradeEvents, t, cfg.ActivityWindow);
        double volumeRate = QuantityRate(tradeEvents, t, cfg.ActivityWindow);
        double tradeBurst = BurstRatio(tradeEvents, t);
        double bboRate = Rate(bboEvents, t, cfg.ActivityWindow);

        double rv5 = RealizedVol(t, cfg.VolShortWindow);
        double rv30 = RealizedVol(t, cfg.VolLongWindow);
        long range5 = MidRange(t, cfg.VolShortWindow);
        long range30 = MidRange(t, cfg.VolLongWindow);
        double jumpRate = MultiTickJumpRate(t, cfg.VolLongWindow);

        double touchDepth = TouchDepth(book);
        double top5Depth = TopDepth(book, 5);
        touchDepthBaseline = Ewma(touchDepthBaseline, touchDepth, 0.03);
        top5DepthBaseline = Ewma(top5DepthBaseline, top5Depth, 0.03);
        eventRateBaseline = Ewma(eventRateBaseline, Math.Max(0.001, bookRate), 0.02);
        rvBaseline = Ewma(rvBaseline, Math.Max(0.001, rv30), 0.02);

        double touchRatio = Ratio(touchDepth, touchDepthBaseline);
        double top5Ratio = Ratio(top5Depth, top5DepthBaseline);
        double cancelPressure = CancellationPressure(book);

        double activityScore = Clamp01(bookRate / Math.Max(1.0, eventRateBaseline * 3.0));
        double volatilityScore = Clamp01(rv30 / Math.Max(1.0, rvBaseline * 3.0) + jumpRate * 0.2);
        double liquidityStress = Clamp01(
            Math.Max(0, meanSpread30 - cfg.OneTickSpread) / 6.0 +
            Math.Max(0, 1.0 - touchRatio) * 0.35 +
            Math.Max(0, 1.0 - top5Ratio) * 0.25 +
            cancelPressure * 0.35);

        var quality = (t - epochStartUtc) >= cfg.WarmupDuration ? MetricQuality.Derived : MetricQuality.WarmingUp;
        string qualityReason = quality == MetricQuality.WarmingUp ? "market-state warmup" : "derived from book/trade stream";
        var desired = Classify(spread, activityScore, volatilityScore, liquidityStress);
        string regimeReason = RegimeReason(desired, spread, activityScore, volatilityScore, liquidityStress);
        ApplyRegimeTransition(desired, regimeReason, t, book.BookEpoch, activityScore, volatilityScore, liquidityStress);

        var risk = liquidityStress >= 0.80 || currentRegime == MarketRegime.VolatileDislocated ? RiskEnvironment.Critical
            : liquidityStress >= 0.60 || volatilityScore >= 0.75 ? RiskEnvironment.Elevated
            : liquidityStress <= 0.25 && volatilityScore <= 0.35 ? RiskEnvironment.Low
            : RiskEnvironment.Normal;

        double confidence = quality == MetricQuality.WarmingUp
            ? Clamp01((t - epochStartUtc).TotalSeconds / Math.Max(1, cfg.WarmupDuration.TotalSeconds))
            : Clamp01(1.0 - Math.Abs(liquidityStress - 0.5) * 0.15);

        Current = new MarketStateSnapshot(
            t,
            book.BookEpoch,
            currentRegime,
            risk,
            quality,
            qualityReason,
            activityScore,
            volatilityScore,
            liquidityStress,
            confidence,
            previousRegime,
            lastTransitionUtc,
            t - lastTransitionUtc,
            lastTransitionReason,
            spread,
            meanSpread30,
            p90Spread60,
            maxSpread60,
            oneTickFraction,
            widenRate,
            narrowRate,
            new SpreadEpisodeSnapshot(episodeActive, episodeActive ? episodeStartedUtc : null,
                episodeActive ? t - episodeStartedUtc : TimeSpan.Zero, episodeStartSpread, episodePeakSpread,
                lastRecoveryDuration),
            touchRatio,
            top5Ratio,
            cancelPressure,
            bboRate,
            bookRate,
            bookBurst,
            tradeRate,
            volumeRate,
            tradeBurst,
            rv5,
            rv30,
            range5,
            range30,
            jumpRate,
            transitions.ToArray());

        previousBook = book;
        previousStats = book.Stats;
        lastSampleUtc = t;
        previousSpread = spread;
        return Current;
    }

    private void TrackBookEventDeltas(BookSnapshot book, DateTime t)
    {
        if (previousBook == null || previousBook.BookEpoch != book.BookEpoch)
            return;

        long delta = Math.Max(0, book.Stats.LiveBookEventCount - previousStats.LiveBookEventCount);
        long qty = Math.Max(0, book.Stats.LiveAddCount + book.Stats.LiveUpdateCount + book.Stats.LiveRemoveCount
            - previousStats.LiveAddCount - previousStats.LiveUpdateCount - previousStats.LiveRemoveCount);
        if (delta > 0)
            bookEvents.Enqueue(new EventSample(t, qty, delta));
        TrimEventQueue(bookEvents, t - cfg.ActivityWindow);
    }

    private void TrackBboChanges(BookSnapshot book, DateTime t)
    {
        if (previousBook?.HasTwoSidedBook == true &&
            (previousBook.BestBidTicks != book.BestBidTicks || previousBook.BestAskTicks != book.BestAskTicks))
        {
            bboEvents.Enqueue(new EventSample(t, 1, 1));
            TrimEventQueue(bboEvents, t - cfg.ActivityWindow);
        }
    }

    private void UpdateSpreadEpisode(long spread, DateTime t)
    {
        bool widened = spread > cfg.OneTickSpread;
        if (widened && !episodeActive)
        {
            episodeActive = true;
            episodeStartedUtc = t;
            episodeStartSpread = spread;
            episodePeakSpread = spread;
        }
        else if (widened && episodeActive)
        {
            episodePeakSpread = Math.Max(episodePeakSpread, spread);
        }
        else if (!widened && episodeActive)
        {
            episodeActive = false;
            lastRecoveryDuration = t - episodeStartedUtc;
        }
    }

    private MarketRegime Classify(long spread, double activity, double vol, double stress)
    {
        if (spread >= cfg.EmergencySpreadTicks || stress >= 0.90)
            return MarketRegime.VolatileDislocated;
        if (episodeActive && stress < 0.45 && currentRegime is MarketRegime.ThinFragile or MarketRegime.VolatileDislocated)
            return MarketRegime.Recovering;
        if (stress >= 0.70 && vol >= 0.55)
            return MarketRegime.VolatileDislocated;
        if (stress >= 0.58)
            return MarketRegime.ThinFragile;
        if (activity >= 0.60 && vol >= 0.45 && stress < 0.55)
            return MarketRegime.FastOrderly;
        if (activity >= 0.38 && stress < 0.50)
            return MarketRegime.ActiveLiquid;
        if (spread <= cfg.OneTickSpread && activity < 0.35 && vol < 0.35 && stress < 0.35)
            return MarketRegime.QuietTight;
        return MarketRegime.ActiveLiquid;
    }

    private void ApplyRegimeTransition(MarketRegime desired, string reason, DateTime t, long bookEpoch, double activity, double vol, double stress)
    {
        if (currentRegime == MarketRegime.Unknown)
        {
            SetRegime(desired, reason, t, bookEpoch, activity, vol, stress);
            return;
        }

        if (desired == currentRegime)
        {
            candidateRegime = MarketRegime.Unknown;
            return;
        }

        bool emergency = desired == MarketRegime.VolatileDislocated && stress >= 0.90;
        bool dwellOk = t - lastTransitionUtc >= cfg.MinimumDwell;
        if (emergency)
        {
            SetRegime(desired, reason, t, bookEpoch, activity, vol, stress);
            return;
        }

        if (candidateRegime != desired)
        {
            candidateRegime = desired;
            candidateSinceUtc = t;
            return;
        }

        if (dwellOk && t - candidateSinceUtc >= cfg.CandidatePersistence)
            SetRegime(desired, reason, t, bookEpoch, activity, vol, stress);
    }

    private void SetRegime(MarketRegime next, string reason, DateTime t, long bookEpoch, double activity, double vol, double stress)
    {
        if (next == currentRegime && lastTransitionUtc != default)
            return;

        previousRegime = currentRegime;
        currentRegime = next;
        lastTransitionUtc = t;
        lastTransitionReason = reason;
        candidateRegime = MarketRegime.Unknown;
        transitions.Add(new MarketStateTransition(t, bookEpoch, previousRegime, next, reason, activity, vol, stress));
        if (transitions.Count > 20)
            transitions.RemoveAt(0);
    }

    private double TimeWeightedMeanSpread(DateTime now, TimeSpan window)
    {
        var samples = spreadSamples.Where(s => s.TimeUtc >= now - window).ToArray();
        if (samples.Length == 0) return 0;
        if (samples.Length == 1) return samples[0].SpreadTicks;
        double sum = 0;
        double total = 0;
        for (int i = 0; i < samples.Length - 1; i++)
        {
            double dt = Math.Max(0.001, (samples[i + 1].TimeUtc - samples[i].TimeUtc).TotalSeconds);
            sum += samples[i].SpreadTicks * dt;
            total += dt;
        }
        double tail = Math.Max(0.001, (now - samples[^1].TimeUtc).TotalSeconds);
        sum += samples[^1].SpreadTicks * tail;
        total += tail;
        return total <= 0 ? samples[^1].SpreadTicks : sum / total;
    }

    private long MaxSpread(DateTime now, TimeSpan window)
        => spreadSamples.Where(s => s.TimeUtc >= now - window).Select(s => s.SpreadTicks).DefaultIfEmpty(0).Max();

    private double PercentileSpread(DateTime now, TimeSpan window, double percentile)
    {
        var values = spreadSamples.Where(s => s.TimeUtc >= now - window).Select(s => s.SpreadTicks).OrderBy(x => x).ToArray();
        if (values.Length == 0) return 0;
        int idx = (int)Math.Ceiling(percentile * values.Length) - 1;
        idx = Math.Clamp(idx, 0, values.Length - 1);
        return values[idx];
    }

    private double TimeAtOneTick(DateTime now, TimeSpan window)
    {
        var values = spreadSamples.Where(s => s.TimeUtc >= now - window).ToArray();
        if (values.Length == 0) return 0;
        return values.Count(s => s.SpreadTicks <= cfg.OneTickSpread) / (double)values.Length;
    }

    private double TransitionRate(DateTime now, bool widened)
    {
        var values = spreadSamples.Where(s => s.TimeUtc >= now - cfg.SpreadDistributionWindow).ToArray();
        if (values.Length < 2) return 0;
        int count = 0;
        for (int i = 1; i < values.Length; i++)
        {
            if (widened && values[i].SpreadTicks > values[i - 1].SpreadTicks) count++;
            if (!widened && values[i].SpreadTicks < values[i - 1].SpreadTicks) count++;
        }
        return count / Math.Max(1.0, cfg.SpreadDistributionWindow.TotalSeconds);
    }

    private double RealizedVol(DateTime now, TimeSpan window)
    {
        var values = midSamples.Where(s => s.TimeUtc >= now - window).ToArray();
        if (values.Length < 2) return 0;
        double sumSq = 0;
        for (int i = 1; i < values.Length; i++)
        {
            double retTicks = (values[i].MidTicks2 - values[i - 1].MidTicks2) / 2.0;
            sumSq += retTicks * retTicks;
        }
        return Math.Sqrt(sumSq);
    }

    private long MidRange(DateTime now, TimeSpan window)
    {
        var values = midSamples.Where(s => s.TimeUtc >= now - window).Select(s => s.MidTicks2).ToArray();
        if (values.Length == 0) return 0;
        return (long)Math.Round((values.Max() - values.Min()) / 2.0);
    }

    private double MultiTickJumpRate(DateTime now, TimeSpan window)
    {
        var values = midSamples.Where(s => s.TimeUtc >= now - window).ToArray();
        if (values.Length < 2) return 0;
        int jumps = 0;
        for (int i = 1; i < values.Length; i++)
            if (Math.Abs(values[i].MidTicks2 - values[i - 1].MidTicks2) / 2.0 >= 2)
                jumps++;
        return jumps / Math.Max(1.0, window.TotalSeconds);
    }

    private double CancellationPressure(BookSnapshot book)
    {
        long adds = book.Stats.LiveAddCount - previousStats.LiveAddCount;
        long removes = book.Stats.LiveRemoveCount - previousStats.LiveRemoveCount;
        return removes <= 0 ? 0 : removes / (double)Math.Max(1, adds + removes);
    }

    private static double TouchDepth(BookSnapshot book)
        => (book.Bids.FirstOrDefault().Quantity + book.Asks.FirstOrDefault().Quantity) / 2.0;

    private static double TopDepth(BookSnapshot book, int depth)
        => (book.Bids.Take(depth).Sum(x => x.Quantity) + book.Asks.Take(depth).Sum(x => x.Quantity)) / 2.0;

    private double BurstRatio(Queue<EventSample> events, DateTime now)
    {
        double shortRate = Rate(events, now, cfg.BurstWindow);
        double longRate = Rate(events, now, cfg.ActivityWindow);
        return longRate <= 0 ? 0 : shortRate / longRate;
    }

    private static double Rate(Queue<EventSample> events, DateTime now, TimeSpan window)
        => events.Where(e => e.TimeUtc >= now - window).Sum(e => e.Count) / Math.Max(1.0, window.TotalSeconds);

    private static double QuantityRate(Queue<EventSample> events, DateTime now, TimeSpan window)
        => events.Where(e => e.TimeUtc >= now - window).Sum(e => e.Quantity) / Math.Max(1.0, window.TotalSeconds);

    private void TrimSpreadQueue(DateTime cutoff)
    {
        while (spreadSamples.Count > 0 && spreadSamples.Peek().TimeUtc < cutoff)
            spreadSamples.Dequeue();
    }

    private void TrimMidQueue(DateTime cutoff)
    {
        while (midSamples.Count > 0 && midSamples.Peek().TimeUtc < cutoff)
            midSamples.Dequeue();
    }

    private static void TrimEventQueue(Queue<EventSample> queue, DateTime cutoff)
    {
        while (queue.Count > 0 && queue.Peek().TimeUtc < cutoff)
            queue.Dequeue();
    }

    private static double Ewma(double baseline, double value, double alpha)
        => baseline <= 0 ? value : baseline + alpha * (value - baseline);

    private static double Ratio(double value, double baseline)
        => baseline <= 0 ? 1.0 : value / baseline;

    private static double Clamp01(double value)
        => Math.Max(0, Math.Min(1, value));

    private static string RegimeReason(MarketRegime regime, long spread, double activity, double vol, double stress)
        => $"{regime}: spread={spread}, activity={activity:F2}, volatility={vol:F2}, liquidityStress={stress:F2}";

    private readonly record struct SpreadSample(DateTime TimeUtc, long SpreadTicks);
    private readonly record struct MidSample(DateTime TimeUtc, double MidTicks2);
    private readonly record struct EventSample(DateTime TimeUtc, double Quantity, double Count);
}
