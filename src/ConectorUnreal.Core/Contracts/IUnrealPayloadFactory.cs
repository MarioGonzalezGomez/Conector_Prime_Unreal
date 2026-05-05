using ConectorUnreal.Core.Models;

namespace ConectorUnreal.Core.Contracts;

public interface IUnrealPayloadFactory
{
    string BuildPayload(MappedCommand command);
}
