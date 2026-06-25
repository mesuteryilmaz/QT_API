namespace QT.Core.Time;

public interface IClock
{
    DateTime UtcNow { get; }
}

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}

public sealed class DeterministicClock : IClock
{
    public DeterministicClock(DateTime startUtc)
    {
        UtcNow = DateTime.SpecifyKind(startUtc, DateTimeKind.Utc);
    }

    public DateTime UtcNow { get; private set; }

    public void Set(DateTime utcNow)
        => UtcNow = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);

    public void Advance(TimeSpan delta)
        => UtcNow = UtcNow.Add(delta);
}
