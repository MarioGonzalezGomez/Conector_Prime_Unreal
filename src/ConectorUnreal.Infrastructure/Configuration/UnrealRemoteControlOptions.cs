namespace ConectorUnreal.Infrastructure.Configuration;

public sealed class UnrealRemoteControlOptions
{
    public string BaseUrl { get; set; } = "http://127.0.0.1:30010";
    public string ActionEndpointUrl { get; set; } = "/remote/preset/Remote/action/B0B58F6C451EF11C3FF6328D95D3470E";
    public string PropertyEndpointUrl { get; set; } = "/remote/preset/Remote/property/8FCE73D44F03DAAF7BF75395687B8B2E";
    public decimal DefaultY { get; set; } = 0;
    public decimal DefaultZ { get; set; } = 0;
    public bool GenerateTransaction { get; set; } = true;
    public int RequestTimeoutSeconds { get; set; } = 5;
}
