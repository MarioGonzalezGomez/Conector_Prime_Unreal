namespace ConectorUnreal.Infrastructure.Configuration;

public sealed class UnrealRemoteControlOptions
{
    public string PropertyEndpointUrl { get; set; } = "http://127.0.0.1:30010/remote/preset/RCP_RA/property/REPLACE_WITH_PROPERTY_ID";
    public decimal DefaultY { get; set; } = 0;
    public decimal DefaultZ { get; set; } = 0;
    public bool GenerateTransaction { get; set; } = true;
    public int RequestTimeoutSeconds { get; set; } = 5;
}
