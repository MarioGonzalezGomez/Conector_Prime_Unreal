namespace ConectorUnreal.App.Configuration;

public sealed class TcpListenerOptions
{
    public string Host { get; set; } = "0.0.0.0";
    public int Port { get; set; } = 8283;
    public int ReadBufferSize { get; set; } = 4096;
}
