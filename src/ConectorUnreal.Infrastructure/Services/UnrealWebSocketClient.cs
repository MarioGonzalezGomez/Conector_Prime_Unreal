using System.Net.WebSockets;
using System.Text;
using ConectorUnreal.Core.Contracts;
using ConectorUnreal.Core.Models;
using ConectorUnreal.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConectorUnreal.Infrastructure.Services;

public sealed class UnrealWebSocketClient : IUnrealRemoteControlClient
{
    private readonly UnrealWebSocketOptions _options;
    private readonly ILogger<UnrealWebSocketClient> _logger;
    private readonly SemaphoreSlim _sync = new(1, 1);
    private ClientWebSocket? _socket;

    public UnrealWebSocketClient(IOptions<UnrealWebSocketOptions> options, ILogger<UnrealWebSocketClient> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public bool IsConnected => _socket?.State == WebSocketState.Open;

    public async Task EnsureConnectedAsync(CancellationToken cancellationToken)
    {
        if (IsConnected)
        {
            return;
        }

        await _sync.WaitAsync(cancellationToken);
        try
        {
            if (IsConnected)
            {
                return;
            }

            _socket?.Dispose();
            _socket = new ClientWebSocket();

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(Math.Max(1, _options.ConnectTimeoutSeconds)));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            await _socket.ConnectAsync(new Uri(_options.Url), linkedCts.Token);
            _logger.LogInformation("Connected to Unreal WebSocket at {Url}", _options.Url);
        }
        finally
        {
            _sync.Release();
        }
    }

    public async Task<UnrealDispatchResult> SendAsync(string payload, CancellationToken cancellationToken)
    {
        try
        {
            await EnsureConnectedAsync(cancellationToken);

            if (_socket is null || _socket.State != WebSocketState.Open)
            {
                return new UnrealDispatchResult(false, "WebSocket is not connected.");
            }

            var bytes = Encoding.UTF8.GetBytes(payload);
            await _socket.SendAsync(bytes, WebSocketMessageType.Text, true, cancellationToken);

            return new UnrealDispatchResult(true, "Payload sent to Unreal.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send payload to Unreal");
            return new UnrealDispatchResult(false, ex.Message);
        }
    }
}
