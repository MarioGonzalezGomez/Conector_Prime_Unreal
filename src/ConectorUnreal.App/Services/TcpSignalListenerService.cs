using System.Net;
using System.Net.Sockets;
using System.Text;
using ConectorUnreal.App.Configuration;
using ConectorUnreal.App.Contracts;
using ConectorUnreal.Core.Contracts;
using ConectorUnreal.Core.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConectorUnreal.App.Services;

public sealed class TcpSignalListenerService : BackgroundService, ITcpListenerStatus
{
    private readonly TcpListenerOptions _options;
    private readonly ISignalProcessor _signalProcessor;
    private readonly ILogger<TcpSignalListenerService> _logger;
    private TcpListener? _listener;

    public TcpSignalListenerService(
        IOptions<TcpListenerOptions> options,
        ISignalProcessor signalProcessor,
        ILogger<TcpSignalListenerService> logger)
    {
        _options = options.Value;
        _signalProcessor = signalProcessor;
        _logger = logger;
    }

    public bool IsListening { get; private set; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var ip = ParseHost(_options.Host);
        _listener = new TcpListener(ip, _options.Port);
        _listener.Start();
        IsListening = true;

        _logger.LogInformation("TCP listener started on {Host}:{Port}", _options.Host, _options.Port);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync(stoppingToken);
                _ = Task.Run(() => HandleClientAsync(client, stoppingToken), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Graceful shutdown.
        }
        finally
        {
            IsListening = false;
            _listener.Stop();
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        try
        {
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8, true, _options.ReadBufferSize, leaveOpen: false);

            while (!cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(cancellationToken);
                if (line is null)
                {
                    break;
                }

                await _signalProcessor.ProcessSignalAsync(line, SignalOrigin.Tcp, cancellationToken);
            }
        }
        catch (IOException ioEx)
        {
            _logger.LogWarning(ioEx, "TCP client disconnected unexpectedly");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while processing TCP client");
        }
        finally
        {
            client.Dispose();
        }
    }

    private static IPAddress ParseHost(string host)
    {
        if (string.Equals(host, "0.0.0.0", StringComparison.OrdinalIgnoreCase) || string.Equals(host, "*", StringComparison.OrdinalIgnoreCase))
        {
            return IPAddress.Any;
        }

        if (IPAddress.TryParse(host, out var ipAddress))
        {
            return ipAddress;
        }

        return IPAddress.Loopback;
    }
}
