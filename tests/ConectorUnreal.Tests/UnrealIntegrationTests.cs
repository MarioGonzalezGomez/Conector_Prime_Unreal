using System.Net;
using System.Text;
using ConectorUnreal.Core.Models;
using ConectorUnreal.Infrastructure.Configuration;
using ConectorUnreal.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace ConectorUnreal.Tests;

public sealed class UnrealIntegrationTests
{
    [Fact]
    public void DictionaryCommandMapper_MapsPlaySumarioAndDynamicTextSignal()
    {
        var mapper = new DictionaryCommandMapper(Options.Create(new CommandMapOptions
        {
            Mappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["P_Sumario"] = "PlaySumario",
                ["CHG_TxtSumario_textovariable"] = "SetTextValue"
            }
        }));

        Assert.True(mapper.TryMap("P_Sumario", out var playCommand, out var playNormalized, out var playError));
        Assert.Equal("PlaySumario", playCommand?.ActionName);
        Assert.Equal("P_Sumario", playNormalized);
        Assert.Null(playError);

        Assert.True(mapper.TryMap("CHG_TxtSumario_HolaMundo", out var textCommand, out var textNormalized, out var textError));
        Assert.Equal("SetTextValue:HolaMundo", textCommand?.ActionName);
        Assert.Equal("CHG_TxtSumario_HolaMundo", textNormalized);
        Assert.Null(textError);
    }

    [Fact]
    public void DefaultUnrealPayloadFactory_BuildsActionAndTextPayloads()
    {
        var factory = new DefaultUnrealPayloadFactory(Options.Create(new UnrealRemoteControlOptions
        {
            ActionEndpointUrl = "http://example.test/action",
            PropertyEndpointUrl = "http://example.test/property",
            GenerateTransaction = true
        }));

        var playPayload = factory.BuildPayload(new MappedCommand("P_Sumario", "PlaySumario"));
        Assert.Contains("\"TargetEndpoint\":\"Action\"", playPayload);
        Assert.Contains("\"Action\":\"PlaySumario\"", playPayload);

        var textPayload = factory.BuildPayload(new MappedCommand("CHG_TxtSumario_HolaMundo", "SetTextValue:HolaMundo"));
        Assert.Contains("\"TargetEndpoint\":\"Property\"", textPayload);
        Assert.Contains("\"PropertyValue\":\"HolaMundo\"", textPayload);
    }

    [Fact]
    public async Task UnrealRemoteControlHttpClient_UsesActionEndpointForPlaySumario()
    {
        var handler = new RecordingHttpMessageHandler();
        var httpClient = new HttpClient(handler);
        var client = new UnrealRemoteControlHttpClient(
            httpClient,
            Options.Create(new UnrealRemoteControlOptions
            {
                ActionEndpointUrl = "http://example.test/action",
                PropertyEndpointUrl = "http://example.test/property",
                RequestTimeoutSeconds = 5
            }),
            NullLogger<UnrealRemoteControlHttpClient>.Instance);

        var result = await client.SendAsync("{\"TargetEndpoint\":\"Action\",\"Action\":\"PlaySumario\"}", CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("http://example.test/action", handler.LastRequestUri);
        Assert.Equal("PUT", handler.LastMethod);
        Assert.NotNull(handler.LastRequestBody);
        Assert.Equal("{\"TargetEndpoint\":\"Action\",\"Action\":\"PlaySumario\"}", handler.LastRequestBody);
    }

    private sealed class RecordingHttpMessageHandler : HttpMessageHandler
    {
        public string? LastRequestUri { get; private set; }
        public string? LastMethod { get; private set; }
        public string? LastRequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequestUri = request.RequestUri?.ToString();
            LastMethod = request.Method.Method;
            LastRequestBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"ok\":true}", Encoding.UTF8, "application/json")
            };

            return response;
        }
    }
}
