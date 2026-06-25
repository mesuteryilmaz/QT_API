namespace QT.Core.Quality;

public enum MetricQuality : byte
{
    Unavailable = 0,
    WarmingUp = 1,
    Derived = 2,
    Exact = 3,
    Stale = 4,
    Invalid = 5
}

public readonly record struct MetricValue<T>(
    T Value,
    MetricQuality Quality,
    DateTime EventTimeUtc,
    string? Reason)
{
    public bool IsUsable => Quality is MetricQuality.Exact or MetricQuality.Derived;

    public static MetricValue<T> Unavailable(DateTime eventTimeUtc, string? reason = null)
        => new(default!, MetricQuality.Unavailable, eventTimeUtc, reason);

    public static MetricValue<T> WarmingUp(DateTime eventTimeUtc, string? reason = null)
        => new(default!, MetricQuality.WarmingUp, eventTimeUtc, reason);

    public static MetricValue<T> Derived(T value, DateTime eventTimeUtc, string? reason = null)
        => new(value, MetricQuality.Derived, eventTimeUtc, reason);

    public static MetricValue<T> Exact(T value, DateTime eventTimeUtc, string? reason = null)
        => new(value, MetricQuality.Exact, eventTimeUtc, reason);

    public static MetricValue<T> Stale(DateTime eventTimeUtc, string? reason = null)
        => new(default!, MetricQuality.Stale, eventTimeUtc, reason);

    public static MetricValue<T> Invalid(DateTime eventTimeUtc, string? reason = null)
        => new(default!, MetricQuality.Invalid, eventTimeUtc, reason);
}

public readonly record struct FeatureValue(
    string Key,
    string DisplayName,
    double Value,
    MetricQuality Quality,
    DateTime EventTimeUtc,
    string Unit,
    string? Reason)
{
    public bool IsNumeric => Quality is MetricQuality.Exact or MetricQuality.Derived;

    public static FeatureValue Unavailable(string key, string displayName, DateTime eventTimeUtc, string unit = "", string? reason = null)
        => new(key, displayName, 0, MetricQuality.Unavailable, eventTimeUtc, unit, reason);

    public static FeatureValue Warming(string key, string displayName, DateTime eventTimeUtc, string unit = "", string? reason = null)
        => new(key, displayName, 0, MetricQuality.WarmingUp, eventTimeUtc, unit, reason);

    public static FeatureValue Derived(string key, string displayName, double value, DateTime eventTimeUtc, string unit = "", string? reason = null)
        => new(key, displayName, value, MetricQuality.Derived, eventTimeUtc, unit, reason);

    public static FeatureValue Exact(string key, string displayName, double value, DateTime eventTimeUtc, string unit = "", string? reason = null)
        => new(key, displayName, value, MetricQuality.Exact, eventTimeUtc, unit, reason);

    public static FeatureValue Stale(string key, string displayName, DateTime eventTimeUtc, string unit = "", string? reason = null)
        => new(key, displayName, 0, MetricQuality.Stale, eventTimeUtc, unit, reason);

    public static FeatureValue Invalid(string key, string displayName, DateTime eventTimeUtc, string unit = "", string? reason = null)
        => new(key, displayName, 0, MetricQuality.Invalid, eventTimeUtc, unit, reason);
}
