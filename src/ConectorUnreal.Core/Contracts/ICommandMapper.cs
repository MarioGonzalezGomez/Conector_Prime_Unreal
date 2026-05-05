using ConectorUnreal.Core.Models;

namespace ConectorUnreal.Core.Contracts;

public interface ICommandMapper
{
    bool TryMap(string rawSignal, out MappedCommand? command, out string normalizedSignal, out string? error);

    IReadOnlyCollection<string> KnownSignals { get; }
}
