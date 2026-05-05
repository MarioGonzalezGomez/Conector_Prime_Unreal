using ConectorUnreal.Core.Models;

namespace ConectorUnreal.Core.Contracts;

public interface ISignalProcessor
{
    Task ProcessSignalAsync(string rawSignal, SignalOrigin origin, CancellationToken cancellationToken);
}
