using ConectorUnreal.Core.Models;

namespace ConectorUnreal.Core.Contracts;

public interface IUnrealRemoteControlClient
{
    bool IsConnected { get; }

    Task<UnrealDispatchResult> SendAsync(string payload, CancellationToken cancellationToken);

    Task EnsureConnectedAsync(CancellationToken cancellationToken);
}
