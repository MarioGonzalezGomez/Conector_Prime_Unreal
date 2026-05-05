namespace ConectorUnreal.Core.Models;

public sealed record SignalEvent(
    DateTimeOffset TimestampUtc,
    SignalOrigin Origin,
    SignalStage Stage,
    string Message,
    string? RawSignal = null,
    string? NormalizedSignal = null,
    string? ActionName = null,
    string? JsonPayload = null,
    bool? Success = null);
