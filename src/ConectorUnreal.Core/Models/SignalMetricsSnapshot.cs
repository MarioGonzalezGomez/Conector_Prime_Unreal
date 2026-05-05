namespace ConectorUnreal.Core.Models;

public sealed record SignalMetricsSnapshot(
    long TotalEvents,
    long ReceivedSignals,
    long SentSignals,
    long ErrorSignals,
    DateTimeOffset? LastEventUtc);
