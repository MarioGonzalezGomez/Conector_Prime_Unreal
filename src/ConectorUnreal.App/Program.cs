using ConectorUnreal.App.Configuration;
using ConectorUnreal.App.Contracts;
using ConectorUnreal.App.Services;
using ConectorUnreal.Core.Contracts;
using ConectorUnreal.Core.Models;
using ConectorUnreal.Infrastructure.Configuration;
using ConectorUnreal.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<TcpListenerOptions>(builder.Configuration.GetSection("TcpListener"));
builder.Services.Configure<CommandMapOptions>(builder.Configuration.GetSection("CommandMap"));
builder.Services.Configure<SignalStoreOptions>(builder.Configuration.GetSection("SignalStore"));
builder.Services.Configure<UnrealWebSocketOptions>(builder.Configuration.GetSection("UnrealWebSocket"));

builder.Services.AddSingleton<ICommandMapper, DictionaryCommandMapper>();
builder.Services.AddSingleton<IUnrealPayloadFactory, DefaultUnrealPayloadFactory>();
builder.Services.AddSingleton<IUnrealRemoteControlClient, UnrealWebSocketClient>();

builder.Services.AddSingleton<ISignalEventStore>(sp =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<SignalStoreOptions>>().Value;
    return new InMemorySignalEventStore(options.MaxEvents);
});

builder.Services.AddSingleton<ISignalProcessor, SignalProcessor>();
builder.Services.AddSingleton<TcpSignalListenerService>();
builder.Services.AddSingleton<ITcpListenerStatus>(sp => sp.GetRequiredService<TcpSignalListenerService>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<TcpSignalListenerService>());

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/health", (ITcpListenerStatus tcpStatus, IUnrealRemoteControlClient unrealClient) => Results.Ok(new
{
    UtcNow = DateTimeOffset.UtcNow,
    TcpListening = tcpStatus.IsListening,
    UnrealConnected = unrealClient.IsConnected
}));

app.MapGet("/api/commands", (ICommandMapper mapper) => Results.Ok(mapper.KnownSignals.OrderBy(x => x)));

app.MapGet("/api/events", (ISignalEventStore store, int? take) =>
{
    var events = store.GetRecent(take ?? 100);
    return Results.Ok(events);
});

app.MapGet("/api/metrics", (ISignalEventStore store) => Results.Ok(store.GetMetrics()));

app.MapPost("/api/manual", async (ManualSignalRequest request, ISignalProcessor processor, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Signal))
    {
        return Results.BadRequest(new { Error = "Signal is required." });
    }

    await processor.ProcessSignalAsync(request.Signal, SignalOrigin.Manual, cancellationToken);
    return Results.Accepted();
});

app.MapPost("/api/unreal/reconnect", async (IUnrealRemoteControlClient client, CancellationToken cancellationToken) =>
{
    await client.EnsureConnectedAsync(cancellationToken);
    return Results.Ok(new { Connected = client.IsConnected });
});

app.Run();

public sealed record ManualSignalRequest(string Signal);
