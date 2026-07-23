using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using ConectorUnreal.Core.Contracts;
using ConectorUnreal.Core.Models;
using ConectorUnreal.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConectorUnreal.Infrastructure.Services;

public sealed class UnrealRemoteControlHttpClient : IUnrealRemoteControlClient
{
    private readonly HttpClient _httpClient;
    private readonly UnrealRemoteControlOptions _options;
    private readonly ILogger<UnrealRemoteControlHttpClient> _logger;

    public UnrealRemoteControlHttpClient(
        HttpClient httpClient,
        IOptions<UnrealRemoteControlOptions> options,
        ILogger<UnrealRemoteControlHttpClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        _httpClient.Timeout = TimeSpan.FromSeconds(Math.Max(1, _options.RequestTimeoutSeconds));
    }

    public bool IsConnected { get; private set; }

    public async Task EnsureConnectedAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Head, _options.PropertyEndpointUrl);
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            // Even 404/405 means host is reachable.
            IsConnected = response.StatusCode != HttpStatusCode.ServiceUnavailable;
        }
        catch (Exception ex)
        {
            IsConnected = false;
            _logger.LogWarning(ex, "Unreal endpoint probe failed.");
        }
    }

    public async Task<UnrealDispatchResult> SendAsync(string payload, CancellationToken cancellationToken)
    {
        try
        {
            using var payloadDocument = JsonDocument.Parse(payload);
            var targetEndpoint = payloadDocument.RootElement.TryGetProperty("TargetEndpoint", out var targetProperty)
                ? targetProperty.GetString()
                : "Property";

            var endpointPath = string.Equals(targetEndpoint, "Action", StringComparison.OrdinalIgnoreCase)
                ? _options.ActionEndpointUrl
                : _options.PropertyEndpointUrl;

            var endpointUrl = BuildAbsoluteUrl(endpointPath);

            using var request = new HttpRequestMessage(HttpMethod.Put, endpointUrl)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            _logger.LogInformation("Sending Unreal payload to {EndpointUrl} with target {TargetEndpoint}", endpointUrl, targetEndpoint);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogInformation("Unreal response {StatusCode}: {ResponseBody}", (int)response.StatusCode, responseBody);

            IsConnected = response.IsSuccessStatusCode;

            if (response.IsSuccessStatusCode)
            {
                return new UnrealDispatchResult(true, $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}", responseBody);
            }

            var detail = $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}";
            return new UnrealDispatchResult(false, detail, responseBody);
        }
        catch (Exception ex)
        {
            IsConnected = false;
            _logger.LogError(ex, "Failed to send PUT request to Unreal Remote Control API.");
            return new UnrealDispatchResult(false, ex.Message);
        }
    }

    private Uri BuildAbsoluteUrl(string endpointPath)
    {
        if (Uri.TryCreate(endpointPath, UriKind.Absolute, out var absoluteUri))
        {
            return absoluteUri;
        }

        if (Uri.TryCreate(_options.BaseUrl, UriKind.Absolute, out var baseUri))
        {
            return new Uri(baseUri, endpointPath.TrimStart('/'));
        }

        throw new InvalidOperationException("Invalid Unreal Remote Control base URL configuration.");
    }
}
