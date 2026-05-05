using System.Collections.Concurrent;
using ConectorUnreal.Core.Contracts;
using ConectorUnreal.Core.Models;

namespace ConectorUnreal.Infrastructure.Services;

public sealed class InMemorySignalEventStore : ISignalEventStore
{
    private readonly ConcurrentQueue<SignalEvent> _events = new();
    private readonly int _maxEvents;
    private long _totalEvents;
    private long _receivedSignals;
    private long _sentSignals;
    private long _errorSignals;
    private DateTimeOffset? _lastEventUtc;

    public InMemorySignalEventStore(int maxEvents)
    {
        _maxEvents = Math.Max(50, maxEvents);
    }

    public void Add(SignalEvent signalEvent)
    {
        _events.Enqueue(signalEvent);
        Interlocked.Increment(ref _totalEvents);
        _lastEventUtc = signalEvent.TimestampUtc;

        if (signalEvent.Stage == SignalStage.Received)
        {
            Interlocked.Increment(ref _receivedSignals);
        }

        if (signalEvent.Stage == SignalStage.Sent)
        {
            Interlocked.Increment(ref _sentSignals);
        }

        if (signalEvent.Stage == SignalStage.Error)
        {
            Interlocked.Increment(ref _errorSignals);
        }

        while (_events.Count > _maxEvents)
        {
            _events.TryDequeue(out _);
        }
    }

    public IReadOnlyList<SignalEvent> GetRecent(int take)
    {
        var limit = Math.Clamp(take, 1, _maxEvents);
        return _events.ToArray().TakeLast(limit).ToList();
    }

    public SignalMetricsSnapshot GetMetrics()
    {
        return new SignalMetricsSnapshot(
            TotalEvents: Interlocked.Read(ref _totalEvents),
            ReceivedSignals: Interlocked.Read(ref _receivedSignals),
            SentSignals: Interlocked.Read(ref _sentSignals),
            ErrorSignals: Interlocked.Read(ref _errorSignals),
            LastEventUtc: _lastEventUtc);
    }
}
