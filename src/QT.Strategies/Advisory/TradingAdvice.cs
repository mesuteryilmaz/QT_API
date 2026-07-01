namespace QT.Strategies.Advisory;

/// <summary>
/// What the advisor recommends the host do this tick. The advisor is a filter and a
/// governor only: it can keep the host out of bad states and shrink size, but it never
/// originates an entry. The host owns the actual entry trigger and all order submission.
/// </summary>
public enum TradingPosture
{
    /// <summary>No working orders; conditions are not right to trade.</summary>
    Disarmed,

    /// <summary>Conditions permit trading in the given <see cref="TradingStyle"/> and size.</summary>
    Armed,

    /// <summary>Get flat now; a risk condition or adverse transition was detected.</summary>
    Flatten
}

/// <summary>
/// The trading style the current regime favors. This selects <em>how</em> a host that
/// already has an entry signal should express it; it is not itself an entry signal.
/// </summary>
public enum TradingStyle
{
    None,
    PassiveQuote,
    MeanRevert,
    Momentum
}

/// <summary>
/// Immutable advice produced from a <see cref="MarketView"/>. <see cref="SizeMultiplier"/>
/// is a 0..1 scalar relative to the host's own base size. <see cref="Reason"/> is an
/// auditable, human-readable explanation mirroring the market-state transition reason style.
/// </summary>
public sealed record TradingAdvice(
    TradingPosture Posture,
    TradingStyle Style,
    double SizeMultiplier,
    string Reason)
{
    public bool IsArmed => Posture == TradingPosture.Armed;

    public static readonly TradingAdvice Disarm =
        new(TradingPosture.Disarmed, TradingStyle.None, 0, "default disarmed");

    public static TradingAdvice Disarmed(string reason)
        => new(TradingPosture.Disarmed, TradingStyle.None, 0, reason);

    public static TradingAdvice FlattenNow(string reason)
        => new(TradingPosture.Flatten, TradingStyle.None, 0, reason);
}
