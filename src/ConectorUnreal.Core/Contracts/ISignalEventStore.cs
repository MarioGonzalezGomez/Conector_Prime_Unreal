using ConectorUnreal.Core.Models;

namespace ConectorUnreal.Core.Contracts;

public interface ISignalEventStore
{
    void Add(SignalEvent signalEvent);

    IReadOnlyList<SignalEvent> GetRecent(int take);

    SignalMetricsSnapshot GetMetrics();
}
