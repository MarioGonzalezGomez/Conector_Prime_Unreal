using ConectorUnreal.Core.Contracts;
using ConectorUnreal.Core.Models;
using Microsoft.Extensions.Logging;

namespace ConectorUnreal.App.Services;

public sealed class SignalProcessor : ISignalProcessor
{
    private readonly ICommandMapper _commandMapper;
    private readonly IUnrealPayloadFactory _payloadFactory;
    private readonly IUnrealRemoteControlClient _unrealClient;
    private readonly ISignalEventStore _eventStore;
    private readonly ILogger<SignalProcessor> _logger;

    public SignalProcessor(
        ICommandMapper commandMapper,
        IUnrealPayloadFactory payloadFactory,
        IUnrealRemoteControlClient unrealClient,
        ISignalEventStore eventStore,
        ILogger<SignalProcessor> logger)
    {
        _commandMapper = commandMapper;
        _payloadFactory = payloadFactory;
        _unrealClient = unrealClient;
        _eventStore = eventStore;
        _logger = logger;
    }

    public async Task ProcessSignalAsync(string rawSignal, SignalOrigin origin, CancellationToken cancellationToken)
    {
        _eventStore.Add(new SignalEvent(
            TimestampUtc: DateTimeOffset.UtcNow,
            Origin: origin,
            Stage: SignalStage.Received,
            Message: "Signal received",
            RawSignal: rawSignal));

        if (!_commandMapper.TryMap(rawSignal, out var mappedCommand, out var normalizedSignal, out var mapError))
        {
            _eventStore.Add(new SignalEvent(
                TimestampUtc: DateTimeOffset.UtcNow,
                Origin: origin,
                Stage: SignalStage.Error,
                Message: mapError ?? "Could not map signal.",
                RawSignal: rawSignal,
                NormalizedSignal: normalizedSignal,
                Success: false));

            _logger.LogWarning("Signal rejected. Origin={Origin} Raw={RawSignal} Reason={Reason}", origin, rawSignal, mapError);
            return;
        }

        _eventStore.Add(new SignalEvent(
            TimestampUtc: DateTimeOffset.UtcNow,
            Origin: origin,
            Stage: SignalStage.Mapped,
            Message: "Signal mapped",
            RawSignal: rawSignal,
            NormalizedSignal: mappedCommand!.Signal,
            ActionName: mappedCommand.ActionName,
            Success: true));

        var payload = _payloadFactory.BuildPayload(mappedCommand);

        _eventStore.Add(new SignalEvent(
            TimestampUtc: DateTimeOffset.UtcNow,
            Origin: origin,
            Stage: SignalStage.Sent,
            Message: "Payload built and queued for send",
            RawSignal: rawSignal,
            NormalizedSignal: mappedCommand.Signal,
            ActionName: mappedCommand.ActionName,
            JsonPayload: payload,
            Success: true));

        var result = await _unrealClient.SendAsync(payload, cancellationToken);

        _eventStore.Add(new SignalEvent(
            TimestampUtc: DateTimeOffset.UtcNow,
            Origin: origin,
            Stage: result.Success ? SignalStage.Response : SignalStage.Error,
            Message: result.Detail,
            RawSignal: rawSignal,
            NormalizedSignal: mappedCommand.Signal,
            ActionName: mappedCommand.ActionName,
            JsonPayload: payload,
            Success: result.Success));
    }
}
