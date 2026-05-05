# TODO - App C# TCP/IP -> Unreal Remote Control API con interfaz web

## Objetivo

- [ ] Construir una aplicacion en C# que reciba senales TCP/IP, las transforme a JSON para Unreal Remote Control API (WebSocket) y permita control manual desde una interfaz web.

## Arquitectura y decisiones base

- [ ] Confirmar stack: .NET 8 (ASP.NET Core) para backend + UI web integrada.
- [ ] Definir modo de ejecucion: app unica (API + TCP listener + WebSocket client + web UI).
- [ ] Definir estructura de proyectos (por ejemplo: `src/App`, `src/Core`, `tests`).
- [ ] Definir configuracion en `appsettings.json` (TCP port, Unreal WS URL, reintentos, logs, seguridad).

## Protocolo de entrada y mapeo a Unreal

- [ ] Definir formato de entrada TCP (texto plano, UTF-8, delimitador por linea).
- [ ] Definir catalogo de comandos de negocio (ej: `P_Directo`, `P_Publicidad`, etc.).
- [ ] Definir contrato de transformacion: comando entrada -> payload JSON Remote Control API.
- [ ] Definir politica para comandos desconocidos (rechazo, warning, telemetria).
- [ ] Documentar versionado de comandos para cambios futuros sin romper compatibilidad.

## Backend C# (recepcion y envio)

- [ ] Implementar listener TCP con puerto configurable.
- [ ] Implementar parser robusto (trim, case-insensitive, validaciones de longitud/formato).
- [ ] Implementar capa de mapeo comando -> accion Unreal.
- [ ] Implementar generador de payload JSON para cada accion.
- [ ] Implementar cliente WebSocket hacia Unreal (connect/reconnect, timeout, backoff).
- [ ] Implementar envio de mensajes y manejo de respuestas de Unreal.
- [ ] Implementar cola interna opcional para evitar perdida de mensajes en picos.
- [ ] Implementar modo manual (sin TCP) para disparar acciones desde UI.

## Interfaz web (botonera + visor)

- [ ] Crear pagina web de control accesible en red local.
- [ ] Implementar botonera manual de acciones (los mismos comandos del mapeo TCP).
- [ ] Implementar visor en tiempo real de:
- [ ] Senales TCP recibidas.
- [ ] Comandos interpretados.
- [ ] JSON emitidos a Unreal.
- [ ] Respuesta/estado de Unreal (ok/error, codigo, mensaje).
- [ ] Implementar estado de conexion visible (TCP listener activo, WS conectado/desconectado).
- [ ] Implementar acciones de operacion: reconectar WS, limpiar visor, modo pause.
- [ ] Implementar filtro/busqueda en visor y limite de historico.

## Observabilidad y operacion

- [ ] Implementar logging estructurado (console + archivo rotado).
- [ ] Implementar correlation id por evento para trazabilidad extremo a extremo.
- [ ] Exponer metricas basicas (recibidas, enviadas, errores, latencia).
- [ ] Implementar endpoint de healthcheck (`/health`) para supervision.
- [ ] Definir runbook de operacion y recuperacion ante fallos.

## Seguridad minima

- [ ] Limitar bind de TCP/API a interfaces requeridas.
- [ ] Proteger UI web con autenticacion simple (usuario/password) o access key.
- [ ] Implementar allowlist de IP para origen TCP (si aplica).
- [ ] Sanitizar y validar todo input antes de mapear/enviar a Unreal.
- [ ] Evitar exponer secretos en logs/config y usar variables de entorno para credenciales.

## Calidad y pruebas

- [ ] Pruebas unitarias de parser, mapeo y generacion JSON.
- [ ] Pruebas de integracion TCP -> transform -> WebSocket (mock Unreal).
- [ ] Pruebas de UI basicas (render de eventos, botonera, estados).
- [ ] Prueba E2E en entorno real con Unreal Remote Control API.
- [ ] Pruebas de resiliencia (caida de WS, reconexion, comandos invalidos, burst TCP).

## Despliegue

- [ ] Definir despliegue en Windows como servicio (recomendado) o contenedor.
- [ ] Preparar perfil de configuracion por entorno (dev/test/prod).
- [ ] Definir estrategia de actualizacion y rollback.
- [ ] Documentar instalacion paso a paso para operacion tecnica.

## Entregables

- [ ] `SPEC.md` con protocolo TCP, comandos y payloads JSON por accion.
- [ ] `ARCHITECTURE.md` con diagrama y decisiones tecnicas.
- [ ] `RUNBOOK.md` con operacion, monitoreo y troubleshooting.
- [ ] Config de ejemplo (`appsettings.Development.json.example`).
- [ ] Suite minima de pruebas automatizadas en CI.
