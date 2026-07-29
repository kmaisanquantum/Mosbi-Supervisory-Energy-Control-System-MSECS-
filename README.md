# Mosbi Supervisory Energy Control System (MSECS)

A cloud-native, API-first Energy Management Platform for monitoring and managing
**solar energy systems** — the backend foundation for future web, mobile, AI, and
utility integration capabilities.

MSECS is not a SCADA clone. It's a microservices-based supervisory system built around
Clean Architecture, DDD, CQRS, and event-driven communication, designed from day one to
support Industrial IoT protocols (Modbus TCP, MQTT, REST today; OPC UA, IEC 61850, DNP3,
CAN Bus in future phases) and AI-driven analytics (not implemented yet, but the data
models are shaped for it).

**Phase 1 scope:** solar panels, inverters, battery storage, smart meters, weather
sensors, and edge gateways. Utility integrations are explicitly out of scope until a
later phase.

## What's implemented in this pass

| Service | Status | Responsibility |
|---|---|---|
| **Identity** | ✅ Implemented | Auth, JWT + refresh tokens, organizations, RBAC, API keys |
| **Site** | ✅ Implemented | Solar site management: location, weather zone, capacity |
| **Asset** | ✅ Implemented | Equipment inventory: arrays, panels, inverters, batteries, meters, maintenance |
| **Device Registry** | ✅ Implemented | Device provisioning, credentials, protocol config, health |
| **Telemetry** | ✅ Implemented | REST/MQTT/Modbus TCP ingestion → TimescaleDB, RabbitMQ events |
| Alarm | 🗒️ Roadmap | Threshold alarms, offline detection, acknowledgement |
| Notification | 🗒️ Roadmap | Email/SMS/WhatsApp/push |
| Command | 🗒️ Roadmap | Device restart/sync/config push |
| Reporting | 🗒️ Roadmap | Daily/weekly/monthly energy, site performance |
| Analytics | 🗒️ Roadmap | Production/consumption, efficiency, downtime, performance ratio |
| **Design & Recommendation** | 🗒️ Roadmap (next pass) | Pre-sales system sizing + post-install optimization |

See [docs/ROADMAP.md](docs/ROADMAP.md) for the full build sequence.

## Architecture

Every service follows the same four-project Clean Architecture shape:

```
Services/<Name>/
  MSECS.<Name>.Domain/           # Entities, value objects, domain events — no framework deps
  MSECS.<Name>.Application/      # CQRS commands/queries (MediatR), validators (FluentValidation)
  MSECS.<Name>.Infrastructure/   # EF Core, Postgres/TimescaleDB, RabbitMQ, external I/O
  MSECS.<Name>.Api/              # ASP.NET Core controllers, Program.cs, Dockerfile
```

Shared building blocks live under `Shared/`:

- **MSECS.SharedKernel** — `Entity`, `AggregateRoot`, `ValueObject`, `Result`, repository/UoW
  interfaces, domain exceptions. Zero framework dependencies.
- **MSECS.BuildingBlocks** — JWT auth wiring, RabbitMQ event bus, Redis cache, Serilog,
  API versioning, Swagger, rate limiting, health checks, and the shared exception-handling
  / correlation-ID / request-logging middleware every API uses identically.

```
Device Layer → Protocol Connectors → Edge Gateway → Telemetry Ingestion
    → RabbitMQ Event Bus → Microservices → PostgreSQL / TimescaleDB → Redis
    → REST API → Future Mobile & Web Apps
```

### Protocol adapters

`MSECS.Telemetry.ProtocolAdapters` defines a single `IProtocolAdapter` interface
implemented by `ModbusTcpAdapter`, `MqttProtocolAdapter`, and `RestProtocolAdapter`.
All three normalize into the same `TelemetrySample` shape before reaching
`IngestReadingCommand`, so adding OPC UA / IEC 61850 / DNP3 / CAN Bus later means adding
one new adapter class — nothing else in the Telemetry Service changes.

### Multi-tenancy

Every tenant-scoped entity implements `ITenantAware` (`OrganizationId`). Cross-service
authorization uses claim-based ASP.NET Core policies (`SitesRead`, `TelemetryIngest`, etc.)
matching the `permission` claims the Identity Service embeds in its JWTs — see
`MSECS.BuildingBlocks.Auth.PermissionPolicies`.

## Running locally

**Prerequisites:** .NET 9 SDK, Docker + Docker Compose.

```bash
# 1. Bring up infrastructure + all five services
docker compose up -d --build

# 2. Check health
curl http://localhost:5101/health/ready   # Identity
curl http://localhost:5102/health/ready   # Site
curl http://localhost:5103/health/ready   # Asset
curl http://localhost:5104/health/ready   # Device Registry
curl http://localhost:5105/health/ready   # Telemetry
```

| Service | Swagger UI | Port |
|---|---|---|
| Identity | http://localhost:5101/swagger | 5101 |
| Site | http://localhost:5102/swagger | 5102 |
| Asset | http://localhost:5103/swagger | 5103 |
| Device Registry | http://localhost:5104/swagger | 5104 |
| Telemetry | http://localhost:5105/swagger | 5105 |

| Infra | UI / endpoint |
|---|---|
| RabbitMQ management | http://localhost:15672 (msecs / msecs_dev_password) |
| Seq (logs) | http://localhost:5341 |
| MinIO console | http://localhost:9001 (msecs / msecs_dev_password) |

### Typical first-run flow

```bash
# Register an organization + OrgAdmin user, get back a JWT
curl -X POST http://localhost:5101/api/v1/auth/register -H "Content-Type: application/json" -d '{
  "organizationName": "Acme Solar Installers",
  "organizationType": "Installer",
  "email": "owner@acme-solar.test",
  "password": "SuperSecret123",
  "firstName": "Ada",
  "lastName": "Owner"
}'

# Use the returned accessToken as a Bearer token to create a site, register assets,
# provision a device, and start pushing telemetry to /api/v1/telemetry/ingest.
```

### Running without Docker

Each service's `appsettings.Development.json` points at `localhost` ports matching the
compose file's published ports (Postgres 5432, TimescaleDB 5433, Redis 6379, RabbitMQ
5672, Mosquitto 1883). Run infra via `docker compose up -d postgres timescaledb redis
rabbitmq mosquitto` and then `dotnet run` any `*.Api` project directly with
`ASPNETCORE_ENVIRONMENT=Development`.

## Solution structure

Open `MSECS.sln` — it references all 24 projects (5 services × 4 layers, 2 shared
projects, 1 test project). Build with `dotnet build MSECS.sln`.

## Environment note for this repository snapshot

This code was generated in an environment without the .NET SDK installed and without
network access to Microsoft's package feeds, so it has **not** been compiled or run
here. Build/run it in your own environment (`dotnet restore && dotnet build`) or via the
CI workflow in `.github/workflows/ci.yml`. The code follows standard, current package
APIs (EF Core 9, MediatR 12, FluentValidation 11, NModbus 3, MQTTnet 4) — if a package
version has moved on since this was written, bump it in the relevant `.csproj`.

## Security notes for this prototype

- **Change every `CHANGE_ME` value** in `appsettings.json` and the JWT signing key
  before running anywhere but a local machine — these are placeholders, not secrets.
- The `docker-compose.yml` values (`msecs_dev_password`, etc.) are for local development
  only. Production deployments should pull all secrets from a secrets manager (Kubernetes
  Secrets + an external provider, Vault, AWS Secrets Manager, etc.), not appsettings files.
- TLS termination is assumed to happen at an ingress/reverse proxy in front of these
  services; `RequireHttpsMetadata = false` on JWT validation reflects that assumption and
  should be revisited if services are ever exposed without a TLS-terminating proxy.

## License

Prototype — license to be determined by Mosbi.
