namespace QT.Storage.Schemas;

public sealed class RecorderConfig
{
    public const int CurrentSchemaVersion = 2;

    public bool Enabled { get; init; }
    public string OutputPath { get; init; } = "";
    public bool RawEvents { get; init; } = true;
    public bool FeatureSnapshots { get; init; } = true;
    public bool Transitions { get; init; } = true;
    public bool Diagnostics { get; init; } = true;
    public TimeSpan FlushInterval { get; init; } = TimeSpan.FromSeconds(2);

    public void Validate()
    {
        if (Enabled && string.IsNullOrWhiteSpace(OutputPath))
            throw new ArgumentException("Recorder output path is required when recording is enabled.", nameof(OutputPath));
        if (FlushInterval <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(FlushInterval), "Flush interval must be positive.");
    }
}
