using System;

namespace QT.Strategies.Grid;

public sealed record OnePairGridQuoteConfig
{
    public int TargetWidthTicks { get; init; } = 100;
    public int WidthToleranceTicks { get; init; } = 20;
    public int MinPassiveOffsetTicks { get; init; } = 1;
    public int RepriceThresholdTicks { get; init; } = 5;

    public bool IsValid(out string reason)
    {
        if (TargetWidthTicks <= 0)
        {
            reason = "target width must be positive";
            return false;
        }

        if (WidthToleranceTicks < 0)
        {
            reason = "width tolerance cannot be negative";
            return false;
        }

        if (MinPassiveOffsetTicks < 0)
        {
            reason = "passive offset cannot be negative";
            return false;
        }

        if (RepriceThresholdTicks < 1)
        {
            reason = "reprice threshold must be at least one tick";
            return false;
        }

        reason = "";
        return true;
    }
}

public enum OnePairGridQuoteDecisionStatus
{
    Ready,
    InvalidConfiguration,
    InvalidMarket,
    WidthOutsideTolerance
}

public sealed record OnePairGridQuoteDecision(
    OnePairGridQuoteDecisionStatus Status,
    long BidOrderTicks,
    long AskOrderTicks,
    long WidthTicks,
    string Reason)
{
    public bool ShouldQuote => Status == OnePairGridQuoteDecisionStatus.Ready;

    public static OnePairGridQuoteDecision Reject(OnePairGridQuoteDecisionStatus status, string reason)
        => new(status, 0, 0, 0, reason);
}

public static class OnePairGridQuoteEngine
{
    public static OnePairGridQuoteDecision Calculate(long bestBidTicks, long bestAskTicks, OnePairGridQuoteConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (!config.IsValid(out var configReason))
            return OnePairGridQuoteDecision.Reject(OnePairGridQuoteDecisionStatus.InvalidConfiguration, configReason);

        if (bestBidTicks <= 0 || bestAskTicks <= 0)
            return OnePairGridQuoteDecision.Reject(OnePairGridQuoteDecisionStatus.InvalidMarket, "missing best bid or ask");

        if (bestAskTicks <= bestBidTicks)
            return OnePairGridQuoteDecision.Reject(OnePairGridQuoteDecisionStatus.InvalidMarket, "locked or crossed market");

        long minWidth = (bestAskTicks + config.MinPassiveOffsetTicks) - (bestBidTicks - config.MinPassiveOffsetTicks);
        long minAllowedWidth = Math.Max(1, config.TargetWidthTicks - config.WidthToleranceTicks);
        long maxAllowedWidth = config.TargetWidthTicks + config.WidthToleranceTicks;

        if (minWidth > maxAllowedWidth)
        {
            return OnePairGridQuoteDecision.Reject(
                OnePairGridQuoteDecisionStatus.WidthOutsideTolerance,
                $"passive minimum width {minWidth} exceeds maximum allowed {maxAllowedWidth}");
        }

        long width = Math.Max(config.TargetWidthTicks, minWidth);
        if (width < minAllowedWidth)
            width = minAllowedWidth;

        if (width > maxAllowedWidth)
        {
            return OnePairGridQuoteDecision.Reject(
                OnePairGridQuoteDecisionStatus.WidthOutsideTolerance,
                $"desired width {width} exceeds maximum allowed {maxAllowedWidth}");
        }

        long maxPassiveBid = bestBidTicks - config.MinPassiveOffsetTicks;
        long minPassiveAsk = bestAskTicks + config.MinPassiveOffsetTicks;

        double midTicks = (bestBidTicks + bestAskTicks) / 2.0;
        long bidTicks = (long)Math.Floor(midTicks - (width / 2.0));
        long askTicks = bidTicks + width;

        if (bidTicks > maxPassiveBid)
        {
            long shift = bidTicks - maxPassiveBid;
            bidTicks -= shift;
            askTicks -= shift;
        }

        if (askTicks < minPassiveAsk)
        {
            long shift = minPassiveAsk - askTicks;
            bidTicks += shift;
            askTicks += shift;
        }

        if (bidTicks > maxPassiveBid || askTicks < minPassiveAsk)
        {
            return OnePairGridQuoteDecision.Reject(
                OnePairGridQuoteDecisionStatus.WidthOutsideTolerance,
                "cannot keep pair passive inside width tolerance");
        }

        long actualWidth = askTicks - bidTicks;
        if (actualWidth < minAllowedWidth || actualWidth > maxAllowedWidth)
        {
            return OnePairGridQuoteDecision.Reject(
                OnePairGridQuoteDecisionStatus.WidthOutsideTolerance,
                $"actual width {actualWidth} outside allowed range {minAllowedWidth}-{maxAllowedWidth}");
        }

        return new OnePairGridQuoteDecision(
            OnePairGridQuoteDecisionStatus.Ready,
            bidTicks,
            askTicks,
            actualWidth,
            "ready");
    }

    public static bool ShouldReprice(long currentOrderTicks, long desiredOrderTicks, OnePairGridQuoteConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        return Math.Abs(currentOrderTicks - desiredOrderTicks) >= config.RepriceThresholdTicks;
    }
}
