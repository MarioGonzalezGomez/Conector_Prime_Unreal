using System.Text.Json;
using ConectorUnreal.Core.Contracts;
using ConectorUnreal.Core.Models;

namespace ConectorUnreal.Infrastructure.Services;

public sealed class DefaultUnrealPayloadFactory : IUnrealPayloadFactory
{
    public string BuildPayload(MappedCommand command)
    {
        var payload = new
        {
            MessageName = "http",
            Parameters = new
            {
                Url = "/remote/object/call",
                Verb = "PUT",
                Body = new
                {
                    FunctionName = command.ActionName,
                    Signal = command.Signal,
                    SentAtUtc = DateTimeOffset.UtcNow
                }
            }
        };

        return JsonSerializer.Serialize(payload);
    }
}
