using QT.Core.Diagnostics;
using QT.Features.Engine;
using QT.Features.FloatingPairs;
using QT.Features.MarketState;
using QT.Market.Events;

namespace QT.Storage.Features;

public interface IAnalyticsRecorder : IDisposable
{
    bool Enabled { get; }
    string Status { get; }
    void RecordRawEvent(in NormalizedMarketEvent evt, string sessionId, string sourceCommit, string buildConfiguration, string configHash);
    void RecordFeatureSnapshot(FeatureSnapshot snapshot, string sourceCommit, string buildConfiguration);
    void RecordRegimeTransition(MarketStateTransition transition, string sessionId, string symbol, string sourceCommit, string buildConfiguration, string configHash);
    void RecordFloatingPairBreak(FloatingPairBreakEvent breakEvent, string sessionId, string symbol, string sourceCommit, string buildConfiguration, string configHash);
    void RecordDiagnostic(RuntimeDiagnostic diagnostic, string sessionId, string symbol, string sourceCommit, string buildConfiguration, string configHash);
    void Flush();
}

public sealed class NullAnalyticsRecorder : IAnalyticsRecorder
{
    public bool Enabled => false;
    public string Status => "disabled";
    public void RecordRawEvent(in NormalizedMarketEvent evt, string sessionId, string sourceCommit, string buildConfiguration, string configHash) { }
    public void RecordFeatureSnapshot(FeatureSnapshot snapshot, string sourceCommit, string buildConfiguration) { }
    public void RecordRegimeTransition(MarketStateTransition transition, string sessionId, string symbol, string sourceCommit, string buildConfiguration, string configHash) { }
    public void RecordFloatingPairBreak(FloatingPairBreakEvent breakEvent, string sessionId, string symbol, string sourceCommit, string buildConfiguration, string configHash) { }
    public void RecordDiagnostic(RuntimeDiagnostic diagnostic, string sessionId, string symbol, string sourceCommit, string buildConfiguration, string configHash) { }
    public void Flush() { }
    public void Dispose() { }
}
