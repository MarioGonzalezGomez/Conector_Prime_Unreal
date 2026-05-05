namespace ConectorUnreal.Infrastructure.Configuration;

public sealed class CommandMapOptions
{
    public Dictionary<string, string> Mappings { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
