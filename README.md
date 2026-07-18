# LiteQueue.NET

A lightweight, Redis-powered messaging platform for .NET applications — providing message queues, background processing, retry policies, dead-letter queues, and pub/sub messaging.

> **Current Status:** Phase 1 — Foundation. Core queue CRUD, message enqueue/receive, FluentValidation, ProblemDetails, health checks, and Docker Compose are functional. Remaining features (retries, delayed messages, scheduling, pub/sub, SDKs, dashboard) are planned for subsequent phases.

## Quick Start

```bash
docker compose up -d     # Start Redis
dotnet run --project LiteQueue.API
```

Then open `https://localhost:7020/swagger` to explore the API.

## Architecture

```
LiteQueue.Domain        — Domain models, enums, repository interfaces
LiteQueue.Contracts     — Public request/response DTOs
LiteQueue.Application   — Use cases and orchestration
LiteQueue.Infrastructure — Redis implementation (StackExchange.Redis)
LiteQueue.Background    — Background workers (visibility recovery, expiration)
LiteQueue.API           — REST API with Swagger, health checks, validation
```

## Phase 1 — Implemented

- Queue creation, listing, and messaging
- Message enqueue and receive via Redis
- In-flight message tracking with sorted sets
- DLQ for messages exceeding delivery attempts
- FluentValidation on API requests
- ProblemDetails error responses
- Health checks (`/api/health/live`, `/api/health/ready`)
- Docker Compose for Redis development
- Configuration-driven Redis connection

## Planned Features

- Phase 2: Reliable queue core (atomic receive, visibility timeout, long polling)
- Phase 3: Retry policies and dead-letter queues
- Phase 4: Delayed and scheduled messages
- Phase 5: Recurring jobs with cron
- Phase 6: Publish/subscribe messaging
- Phase 7: Client and Worker SDKs (NuGet packages)
- Phase 8: Security and administration
- Phase 9: Angular admin dashboard
- Phase 10: Production hardening (OpenTelemetry, metrics, distributed tracing)

## Delivery Guarantee

> **At least once.** LiteQueue.NET does not promise exactly-once processing.

## Building

```bash
dotnet build
dotnet test
```
