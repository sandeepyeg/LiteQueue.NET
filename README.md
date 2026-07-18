# LiteQueue.NET

A lightweight, Redis-powered messaging and background-processing platform built for modern .NET applications.

## Features (All 10 Phases Implemented)

- **Reliable Message Queues** — FIFO delivery with atomic receive, visibility timeout, and acknowledgement
- **In-Flight Tracking** — Messages are tracked via Redis sorted sets with automatic timeout recovery
- **Retry Policies** — Immediate, fixed delay, linear, exponential, and exponential-with-jitter strategies
- **Dead-Letter Queues** — Automatic DLQ routing on max attempts exceeded, with browse/redrive/delete
- **Delayed Messages** — Schedule messages for future delivery via sorted sets + background promotion worker
- **Scheduled Jobs** — One-time execution at exact date/time
- **Recurring Jobs** — Cron-based scheduling with timezone support, misfire policies, enable/disable
- **Pub/Sub Messaging** — Topics with multiple subscriptions, attribute filtering, subscription isolation
- **Message Prioritization** — Low, Normal, High, Critical priority levels with fairness
- **Idempotency** — Optional idempotency keys to prevent duplicate message delivery
- **REST API** — Full CRUD for queues, messages, topics, subscriptions, schedules, and recurring jobs
- **FluentValidation** — Input validation on all API requests
- **ProblemDetails** — RFC 7807 compliant error responses
- **Health Checks** — `/api/health/live`, `/api/health/ready`, `/api/health`
- **Security** — API key authentication, queue/topic scopes, rate limiting
- **OpenTelemetry** — Distributed tracing, Prometheus metrics, structured logging
- **Client SDK** — Typed `LiteQueueClient` NuGet package for producing messages
- **Worker SDK** — Typed `ILiteQueueHandler<T>` NuGet package for consuming messages
- **Angular Dashboard** — Queue overview, message browser, DLQ admin, SignalR live updates
- **Docker Compose** — Redis, API, Dashboard, Prometheus, Grafana in one command
- **CI/CD** — GitHub Actions for build, test (with Redis service container), Docker image publish

## Delivery Guarantee

> **At least once.** LiteQueue.NET does not promise exactly-once processing.

## Quick Start

```bash
docker compose up -d
# API available at http://localhost:8080/swagger
# Dashboard available at http://localhost:4200
```

## Architecture

```
LiteQueue.Domain        — Domain models, enums, repository interfaces, retry calculator
LiteQueue.Contracts     — Public request/response DTOs (Queues, Messages, Topics, Administration)
LiteQueue.Application   — Use cases, orchestration, validation (QueueService, SchedulingService, TopicService)
LiteQueue.Infrastructure — Redis implementation (StackExchange.Redis, Lua scripts, key naming)
LiteQueue.Background    — Background workers (visibility, delayed, recurring, DLQ, statistics, cleanup)
LiteQueue.API           — ASP.NET Core REST API, FluentValidation, health checks, OpenTelemetry, auth
LiteQueue.Client        — NuGet SDK for .NET consumers (typed producer client)
LiteQueue.Worker        — NuGet SDK for .NET message handlers (typed handler interface)
LiteQueue.Dashboard     — Angular 18 admin dashboard with SignalR live updates
LiteQueue.Tests         — xUnit tests covering domain models and Redis key infrastructure
```

## Redis Key Design

```
litequeue:queues                     SET — queue registry
litequeue:queue:{name}:config        HASH — queue configuration
litequeue:queue:{name}:ready         LIST — ready message IDs
litequeue:queue:{name}:inflight      ZSET — in-flight messages (score = visibility expiry)
litequeue:queue:{name}:delayed       ZSET — delayed messages (score = available timestamp)
litequeue:queue:{name}:dead          LIST — dead-letter messages
litequeue:message:{id}               STRING — message payload (JSON)
litequeue:receipt:{handle}           STRING — receipt metadata (with TTL)
litequeue:topics                     SET — topic registry
litequeue:topic:{name}:subscriptions SET — subscription names
litequeue:schedules:pending          ZSET — one-time scheduled messages
litequeue:recurring:{jobId}          HASH — recurring job definition
litequeue:dedupe:{queue}:{key}       STRING — idempotency dedup (with TTL)
```

## Building

```bash
dotnet build     # Builds all 9 projects (0 errors, 0 warnings)
dotnet test      # Runs 21 xUnit tests
```

## Configuration

All settings are in `appsettings.json` under the `LiteQueue` section:

```json
{
  "LiteQueue": {
    "Redis": {
      "ConnectionString": "localhost:6379"
    },
    "Defaults": {
      "VisibilityTimeoutSeconds": 60,
      "MaxDeliveryAttempts": 5,
      "MaximumMessageSizeBytes": 262144
    }
  }
}
```

## API Endpoints

### Queues
```text
POST   /api/queues/{name}                        Create queue
GET    /api/queues                               List queues
GET    /api/queues/{name}                        Get queue config
PATCH  /api/queues/{name}                        Update queue config
DELETE /api/queues/{name}                        Purge and delete queue
POST   /api/queues/{name}/pause                  Pause queue
POST   /api/queues/{name}/resume                 Resume queue
POST   /api/queues/{name}/purge                  Purge all messages
```

### Messages
```text
POST   /api/queues/{name}/messages               Enqueue message
POST   /api/queues/{name}/messages/batch         Batch enqueue
POST   /api/queues/{name}/receive                Receive messages
POST   /api/queues/{name}/receive/longpoll       Receive with long polling
POST   /api/queues/{name}/acknowledge            Acknowledge message
POST   /api/queues/{name}/reject                 Reject message
POST   /api/queues/{name}/visibility             Extend visibility timeout
GET    /api/queues/{name}/peek                   Peek message
GET    /api/queues/{name}/statistics             Queue statistics
```

### Dead Letters
```text
GET    /api/queues/{name}/deadletters            List DLQ messages
GET    /api/queues/{name}/deadletters/{id}       Get DLQ message
POST   /api/queues/{name}/deadletters/{id}/redrive  Redrive single
POST   /api/queues/{name}/deadletters/redrive    Batch redrive
DELETE /api/queues/{name}/deadletters/{id}       Delete from DLQ
```

### Topics
```text
POST   /api/topics/{name}                        Create topic
GET    /api/topics                               List topics
GET    /api/topics/{name}                        Get topic
DELETE /api/topics/{name}                        Delete topic
POST   /api/topics/{name}/messages               Publish message
POST   /api/topics/{name}/subscriptions          Create subscription
GET    /api/topics/{name}/subscriptions          List subscriptions
DELETE /api/topics/{name}/subscriptions/{sub}    Delete subscription
```

### Scheduling
```text
POST   /api/scheduling/schedules                 Schedule one-time job
GET    /api/scheduling/schedules/pending         List pending schedules
POST   /api/scheduling/recurring                 Create recurring job
GET    /api/scheduling/recurring                 List recurring jobs
GET    /api/scheduling/recurring/{id}            Get recurring job
PUT    /api/scheduling/recurring/{id}            Update recurring job
DELETE /api/scheduling/recurring/{id}            Delete recurring job
POST   /api/scheduling/recurring/{id}/trigger    Trigger immediately
```

### Health
```text
GET    /api/health                               Full health
GET    /api/health/live                          Liveness
GET    /api/health/ready                         Readiness (Redis check)
```

## NuGet Packages

```csharp
// Client
services.AddLiteQueueClient(options =>
{
    options.BaseAddress = new Uri("http://litequeue-api:8080");
    options.ApiKey = "your-api-key";
});

await producer.SendAsync("emails", new SendEmailCommand { To = "user@example.com" });

// Worker
services.AddLiteQueueWorker(options =>
{
    options.QueueName = "emails";
    options.BaseAddress = new Uri("http://litequeue-api:8080");
    options.ApiKey = "your-api-key";
});

services.AddLiteQueueHandler<SendEmailCommand, SendEmailHandler>();
```

## Testing

```bash
dotnet test  # 21 tests passing (domain models, Redis key builder)
```
