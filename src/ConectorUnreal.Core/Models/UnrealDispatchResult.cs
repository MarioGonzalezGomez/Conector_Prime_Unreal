namespace ConectorUnreal.Core.Models;

public sealed record UnrealDispatchResult(bool Success, string Detail, string? RawResponse = null);
