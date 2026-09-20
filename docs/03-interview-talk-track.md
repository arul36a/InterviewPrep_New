# Interview talk track

## “Explain the HLD”

Use the README one-minute version, then offer to go deep on **one** of: async jobs, tenant isolation, or seasonal scale.

## “Why microservices, not one calc API?”

- Independent deploy of TaxCalc vs workflow vs reference data
- Seasonal scale of compute-heavy TaxCalc without scaling CoA
- Separate failure domains (StateMods outage should not block CoA reads)
- Trade-off: distributed transactions, eventual job state, operational cost

## “Why MediatR?”

Controllers stay HTTP-shaped. New calc operations are new requests/handlers. Handlers are unit-testable without spinning Kestrel. Matches ETP ServiceHost: Controller → Handler → Provider → Repository.

## “Why Mongo?”

Variable tax documents, evolving schema, tenant-sized artifacts. Shard/partition discussion: `EtpContainerId` + `ClientId` + `ProjectId`. Migrations are dated scripts (`EnsureTenantIndexesMigration`), not EF migrations.

## “Sync vs async?”

Immediate read/create = REST. Ten-minute calc = `202` + job id + bus + poll/SignalR (SignalR is talk-track; this scaffold polls).

## “How do you not leak tenant data?”

`ITenantContextAccessor` is `AsyncLocal`. Repositories **always** AND tenant filters. Cache keys include `ShardKey`. Never accept container id only from the body — headers/claims are the source of truth.

## “What fails in production that this demo hides?”

- In-memory bus is per process; use Redis/Service Bus for fan-out
- No outbox; crash after calc but before `CalculationFinished` can desync JobTracker
- No idempotent consumers
- No Event Hub / SignalR
- Auth is a dev bearer, not APIM JWT validation
- No real multi-region residency

## “NFR?”

Mechanisms yes, SLAs no. See [02-nfr.md](02-nfr.md).
