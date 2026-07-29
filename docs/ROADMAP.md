# MSECS Development Roadmap

## Pass 1 — Foundation (this pass) ✅

- Solution scaffold, Clean Architecture conventions, SharedKernel, BuildingBlocks
- Docker Compose: Postgres, TimescaleDB, Redis, RabbitMQ, Mosquitto, MinIO, Seq
- **Identity Service**: registration, login, JWT + refresh token rotation, RBAC,
  organizations, API keys, seeded system roles/permissions
- **Site Service**: solar site CRUD, GPS coordinates, weather zone, capacity
- **Asset Service**: equipment inventory (arrays, panels, inverters, batteries, meters,
  weather stations), maintenance history
- **Device Registry**: device provisioning + credential issuance, Modbus/MQTT/REST
  connection config, health status
- **Telemetry Service**: REST ingestion endpoint, Modbus TCP polling background service,
  MQTT push subscription, TimescaleDB hypertable with compression + retention policies,
  RabbitMQ event publishing on ingest
- CI workflow (build + test + per-service Docker image build)
- Unit test project + example handler tests for Identity

## Pass 2 — Design & Recommendation Service (next)

Covers both pre-sales and post-install use cases:

- **Pre-sales system design**: given site coordinates + a load profile, recommend panel
  count/layout, inverter sizing, and battery sizing using engineering formulas (not ML)
  against irradiance data and equipment specs from the Asset Service's catalog data.
- **Post-install optimization**: given a site's historical Telemetry data and Asset
  ratings, recommend battery dispatch strategy, flag under-performing strings/inverters
  (comparing actual vs. expected production), and suggest upgrades.
- Data model shaped so a future ML model (Pass 5+) can replace the rule engine without
  changing the API contract.

## Pass 3 — Operational services

- **Alarm Service**: threshold rules (per-asset, per-metric), offline-device detection
  (consuming Device Registry health events), alarm acknowledgement workflow, history
- **Notification Service**: email (SMTP/SES) and SMS (Twilio) delivery, subscribing to
  Alarm Service events; WhatsApp/push deferred to a later pass per original spec
- **Command Service**: device restart/sync/config-update commands issued through the
  Device Registry's provisioned credentials, with command status tracking

## Pass 4 — Reporting & Analytics

- **Reporting Service**: daily/weekly/monthly energy rollups (TimescaleDB continuous
  aggregates), site performance reports, battery health reports, savings estimates
- **Analytics Service**: production/consumption trends, peak production, efficiency,
  downtime tracking, performance ratio — consuming Telemetry + Asset + Design service data

## Pass 5 — AI-ready data activation (still not "AI yet")

- Materialize the predictive-maintenance, battery-health-prediction, solar-forecasting,
  fault-detection, and anomaly-detection feature tables the Phase 1 schema was designed
  to support (TelemetryMetricType enum, MaintenanceRecord history, Design service outputs)
- No model training/inference in this pass — just the feature store and labeling pipeline

## Pass 6 — Frontend

- Web app (installer + site-owner dashboards) and mobile app, built only after backend
  API contracts are stable per the original project mandate ("No frontend should be
  developed until the backend architecture is complete")

## Pass 7 — Utility integration (out of scope until then)

- Utility API adapters, MCP servers, protocol adapters for grid-side integration —
  explicitly deferred; Phase 1 is solar-only by design

## Known gaps to close before any pass is "production" (not just prototype)

- Cross-service device→asset/site/org lookup for Modbus polling currently uses an
  `InMemoryModbusPollTargetProvider` stub (`MSECS.Telemetry.Infrastructure.BackgroundServices`);
  needs a Redis-backed cache populated by subscribing to `DeviceProvisionedEvent`/
  `DeviceRevokedEvent` from the Device Registry over RabbitMQ.
- Integration tests (WebApplicationFactory-based, hitting real Postgres via Testcontainers)
  are not yet included — only unit tests against EF Core InMemory for Identity.
- Refresh tokens are stored in plaintext in this pass; hash-at-rest before production.
- No API gateway / BFF yet — clients currently call each service's public port directly.
  A Kubernetes Ingress + optional gateway (YARP) is the natural next step once there's a
  frontend to serve.
