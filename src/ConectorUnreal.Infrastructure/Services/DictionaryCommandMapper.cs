using ConectorUnreal.Core.Contracts;
using ConectorUnreal.Core.Models;
using ConectorUnreal.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ConectorUnreal.Infrastructure.Services;

public sealed class DictionaryCommandMapper : ICommandMapper
{
    private readonly Dictionary<string, string> _mappings;

    public DictionaryCommandMapper(IOptions<CommandMapOptions> options)
    {
        _mappings = options.Value.Mappings ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (_mappings.Comparer != StringComparer.OrdinalIgnoreCase)
        {
            _mappings = new Dictionary<string, string>(_mappings, StringComparer.OrdinalIgnoreCase);
        }
    }

    public IReadOnlyCollection<string> KnownSignals => _mappings.Keys.ToList().AsReadOnly();

    public bool TryMap(string rawSignal, out MappedCommand? command, out string normalizedSignal, out string? error)
    {
        normalizedSignal = Normalize(rawSignal);

        if (string.IsNullOrWhiteSpace(normalizedSignal))
        {
            command = null;
            error = "Signal is empty.";
            return false;
        }

        if (_mappings.TryGetValue(normalizedSignal, out var actionName))
        {
            command = new MappedCommand(normalizedSignal, actionName);
            error = null;
            return true;
        }

        command = null;
        error = $"Signal '{normalizedSignal}' is not configured.";
        return false;
    }

    private static string Normalize(string rawSignal) => rawSignal.Trim();
}
