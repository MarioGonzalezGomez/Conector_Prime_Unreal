# ConectorUnreal

Base scaffold for a C# application that:

- Receives TCP/IP signals on a configurable port.
- Maps those signals to Unreal actions.
- Transforms and sends payloads to Unreal Remote Control API through WebSocket.
- Exposes a web UI with manual buttons and a live signal monitor.

## Project structure

- `src/ConectorUnreal.App`: Web app (API + TCP listener hosted service + static UI).
- `src/ConectorUnreal.Core`: Domain models and contracts.
- `src/ConectorUnreal.Infrastructure`: Implementations (mapper, payload factory, event store, WS client).
- `tests/ConectorUnreal.Tests`: Test project scaffold.

## Run

```bash
dotnet restore src/ConectorUnreal.App/ConectorUnreal.App.csproj
dotnet run --project src/ConectorUnreal.App/ConectorUnreal.App.csproj
```

Open the UI in your browser:

- `http://localhost:5000` (or the URL shown by `dotnet run`)

## Main endpoints

- `GET /api/health`
- `GET /api/commands`
- `GET /api/events?take=200`
- `GET /api/metrics`
- `POST /api/manual`
- `POST /api/unreal/reconnect`

## Configuration

Edit:

- `src/ConectorUnreal.App/appsettings.json`

Main sections:

- `TcpListener`
- `UnrealWebSocket`
- `SignalStore`
- `CommandMap`
