using System.Globalization;
using System.Text.Json;
using ConectorUnreal.Core.Contracts;
using ConectorUnreal.Core.Models;
using ConectorUnreal.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ConectorUnreal.Infrastructure.Services;

public sealed class DefaultUnrealPayloadFactory : IUnrealPayloadFactory
{
    private const string SetPositionXPrefix = "SetPositionX:";
    private readonly UnrealRemoteControlOptions _options;

    public DefaultUnrealPayloadFactory(IOptions<UnrealRemoteControlOptions> options)
    {
        _options = options.Value;
    }

    public string BuildPayload(MappedCommand command)
    {
        if (command.ActionName.StartsWith(SetPositionXPrefix, StringComparison.Ordinal))
        {
            var rawX = command.ActionName[SetPositionXPrefix.Length..];
            if (!decimal.TryParse(rawX, NumberStyles.Float, CultureInfo.InvariantCulture, out var xValue))
            {
                throw new InvalidOperationException($"Invalid SetPositionX payload for signal '{command.Signal}'.");
            }

            var payload = new
            {
                PropertyValue = new
                {
                    X = xValue,
                    Y = _options.DefaultY,
                    Z = _options.DefaultZ
                },
                GenerateTransaction = _options.GenerateTransaction
            };

            return JsonSerializer.Serialize(payload);
        }

        var fallbackPayload = new
        {
            Signal = command.Signal,
            Action = command.ActionName,
            SentAtUtc = DateTimeOffset.UtcNow
        };

        return JsonSerializer.Serialize(fallbackPayload);
    }
}
