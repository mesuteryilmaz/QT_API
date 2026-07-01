using QT.Core.Primitives;
using QT.Core.Quality;
using QT.Market.Events;
using QT.Market.Snapshots;

namespace QT.Features.OrderFlow;

/// <summary>
/// Infers a directional lean from three transparent, auditable components: aggressor trade imbalance,
/// signed volume delta (CVD) slope, and resting book pressure. Pure and deterministic. The output is
/// a hypothesis with a stated reason — not a validated edge.
/// </summary>
public sealed class OrderFlowMonitor
{
    private readonly OrderFlowConfig cfg;
    private readonly Queue<TradeSample> trades = new();

    private long epoch = -1;
    private DateTime epochStartUtc;
    private long cumulativeSignedVolume;

    public OrderFlowMonitor(OrderFlowConfig? config = null)
    {
        cfg = config ?? new OrderFlowConfig();
        cfg.Validate();
    }

    public OrderFlowSnapshot Current { get; private set; } =
        OrderFlowSnapshot.Empty(DateTime.MinValue, 0, MetricQuality.Unavailable, "not initialized");

    public void Reset(long newEpoch, DateTime eventTimeUtc)
    {
        epoch = newEpoch;
        epochStartUtc = eventTimeUtc;
        cumulativeSignedVolume = 0;
        trades.Clear();
        Current = OrderFlowSnapshot.Empty(eventTimeUtc, newEpoch, MetricQuality.WarmingUp, "epoch reset");
    }

    public void OnMarketEvent(in NormalizedMarketEvent evt)
    {
        if (evt.Kind != MarketEventKind.Trade)
            return;

        long qty = Math.Max(0, evt.Quantity);
        int sign = evt.Aggressor == TradeAggressor.Buy ? 1 : evt.Aggressor == TradeAggressor.Sell ? -1 : 0;
        cumulativeSignedVolume += sign * qty;
        trades.Enqueue(new TradeSample(evt.EventTimeUtc, evt.Aggressor, qty));
        TrimTrades(evt.EventTimeUtc - cfg.LongWindow);
    }

    public OrderFlowSnapshot OnBookSnapshot(BookSnapshot book, DateTime nowUtc)
    {
        if (book.BookEpoch != epoch)
            Reset(book.BookEpoch, book.EventTimeUtc);

        DateTime t = book.EventTimeUtc;
        TrimTrades(t - cfg.LongWindow);

        (long buyShort, long sellShort) = VolumeInWindow(t, cfg.ShortWindow);
        (long buyLong, long sellLong) = VolumeInWindow(t, cfg.LongWindow);

        long signedShort = buyShort - sellShort;
        long signedLong = buyLong - sellLong;
        double shortSec = Math.Max(0.001, cfg.ShortWindow.TotalSeconds);
        double longSec = Math.Max(0.001, cfg.LongWindow.TotalSeconds);

        double cvdSlopePerSec = signedShort / shortSec;
        double buyVolPerSec = buyShort / shortSec;
        double sellVolPerSec = sellShort / shortSec;

        long totalShort = buyShort + sellShort;
        double imbalance = totalShort > 0 ? buyShort / (double)totalShort : 0.5;
        double bookPressure = BookPressure(book, cfg.BookPressureDepth);

        double wSum = cfg.WeightImbalance + cfg.WeightBookPressure + cfg.WeightCvdSlope;
        double imbalanceComponent = (2.0 * imbalance - 1.0) * cfg.WeightImbalance;
        double bookComponent = bookPressure * cfg.WeightBookPressure;
        double cvdComponent = Clamp(cvdSlopePerSec / cfg.CvdSlopeScalePerSec, -1.0, 1.0) * cfg.WeightCvdSlope;
        double leanScore = (imbalanceComponent + bookComponent + cvdComponent) / wSum;

        var quality = (t - epochStartUtc) >= cfg.WarmupDuration ? MetricQuality.Derived : MetricQuality.WarmingUp;
        string qualityReason = quality == MetricQuality.WarmingUp ? "order-flow warmup" : "derived from trade + book flow";

        DirectionalBias bias = quality == MetricQuality.WarmingUp
            ? DirectionalBias.Neutral
            : leanScore >= cfg.LeanThreshold ? DirectionalBias.Up
            : leanScore <= -cfg.LeanThreshold ? DirectionalBias.Down
            : DirectionalBias.Neutral;

        double confidence = quality == MetricQuality.WarmingUp
            ? Clamp((t - epochStartUtc).TotalSeconds / Math.Max(1, cfg.WarmupDuration.TotalSeconds), 0, 1)
            : Clamp(Math.Abs(leanScore) / cfg.StrongLeanThreshold, 0, 1);

        string reason = $"imb={imbalance:F2} book={bookPressure:+0.00;-0.00} cvd={cvdSlopePerSec:+0;-0}/s score={leanScore:+0.00;-0.00} -> {bias}";

        Current = new OrderFlowSnapshot(
            t, book.BookEpoch, quality, qualityReason,
            bias, leanScore, confidence,
            signedShort, signedLong, cvdSlopePerSec,
            imbalance, bookPressure, buyVolPerSec, sellVolPerSec,
            reason);
        return Current;
    }

    private (long buy, long sell) VolumeInWindow(DateTime now, TimeSpan window)
    {
        DateTime cutoff = now - window;
        long buy = 0, sell = 0;
        foreach (var s in trades)
        {
            if (s.TimeUtc < cutoff) continue;
            if (s.Aggressor == TradeAggressor.Buy) buy += s.Quantity;
            else if (s.Aggressor == TradeAggressor.Sell) sell += s.Quantity;
        }
        return (buy, sell);
    }

    private static double BookPressure(BookSnapshot book, int depth)
    {
        if (!book.HasTwoSidedBook)
            return 0.0;

        long bid = 0, ask = 0;
        foreach (var lvl in book.Bids.Take(depth)) bid += lvl.Quantity;
        foreach (var lvl in book.Asks.Take(depth)) ask += lvl.Quantity;
        long total = bid + ask;
        return total > 0 ? (bid - ask) / (double)total : 0.0;
    }

    private void TrimTrades(DateTime cutoff)
    {
        while (trades.Count > 0 && trades.Peek().TimeUtc < cutoff)
            trades.Dequeue();
    }

    private static double Clamp(double value, double min, double max)
        => Math.Max(min, Math.Min(max, value));

    private readonly record struct TradeSample(DateTime TimeUtc, TradeAggressor Aggressor, long Quantity);
}
