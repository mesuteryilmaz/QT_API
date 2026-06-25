namespace QT.Core.Diagnostics;

public enum DiagnosticSeverity : byte
{
    Info = 0,
    Warning = 1,
    Critical = 2
}

public readonly record struct RuntimeDiagnostic(
    DateTime TimeUtc,
    DiagnosticSeverity Severity,
    string Component,
    string Code,
    string Message,
    long BookEpoch);
