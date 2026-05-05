namespace ConectorUnreal.Infrastructure.Configuration;

public sealed class UnrealWebSocketOptions
{
    public string Url { get; set; } = "ws://127.0.0.1:30020";
    public int ConnectTimeoutSeconds { get; set; } = 3;
    public bool AutoConnectOnStartup { get; set; } = false;
}
