using System.Globalization;
using System.Text.RegularExpressions;
using ConectorUnreal.Core.Contracts;
using ConectorUnreal.Core.Models;
using ConectorUnreal.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ConectorUnreal.Infrastructure.Services;

public sealed class DictionaryCommandMapper : ICommandMapper
{
    private static readonly Regex PositionXRegex = new(
        "^CHP_Posicion_X_(-?\\d+(?:[\\.,]\\d+)?)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private readonly Dictionary<string, string> _mappings;

    public DictionaryCommandMapper(IOptions<CommandMapOptions> options)
    {
        _mappings = options.Value.Mappings ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (_mappings.Comparer != StringComparer.OrdinalIgnoreCase)
        {
            _mappings = new Dictionary<string, string>(_mappings, StringComparer.OrdinalIgnoreCase);
        }
    }

    public IReadOnlyCollection<string> KnownSignals => _mappings.Keys
        .Concat(new[] { "CHP_Posicion_X_<valor>" })
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderBy(x => x)
        .ToList()
        .AsReadOnly();

    public bool TryMap(string rawSignal, out MappedCommand? command, out string normalizedSignal, out string? error)
    {
        normalizedSignal = Normalize(rawSignal);

        if (string.IsNullOrWhiteSpace(normalizedSignal))
        {
            command = null;
            error = "Signal is empty.";
            return false;
        }

        var positionMatch = PositionXRegex.Match(normalizedSignal);
        if (positionMatch.Success)
        {
            var rawValue = positionMatch.Groups[1].Value.Replace(',', '.');
            if (decimal.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var xValue))
            {
                command = new MappedCommand(normalizedSignal, $"SetPositionX:{xValue.ToString(CultureInfo.InvariantCulture)}");
                error = null;
                return true;
            }

            command = null;
            error = $"Invalid X value in signal '{normalizedSignal}'.";
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
