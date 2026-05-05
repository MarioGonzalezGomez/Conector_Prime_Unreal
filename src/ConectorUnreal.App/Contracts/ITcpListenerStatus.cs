namespace ConectorUnreal.App.Contracts;

public interface ITcpListenerStatus
{
    bool IsListening { get; }
}
