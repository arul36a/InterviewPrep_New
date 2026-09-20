# Non-functional requirements

The original Calc2 source **does not ship a formal NFR document**. Treat numbers below as **design proxies**, not approved SLAs.

## What the architecture addresses

| NFR | Mechanism in this scaffold |
| --- | --- |
| Scalability | Stateless ServiceHosts; KEDA sample; no session affinity |
| Availability | `/liveness`, `/readiness`, default 2 replicas in k8s sample |
| Resilience | Polly retry + circuit breaker on internal HTTP; graceful drain via `IInFlightWork` |
| Performance | Cache-aside (default **15 minutes**); async jobs; response compression |
| Security | Optional `Auth:Enabled` + `etp_core` analogue; k8s `automountServiceAccountToken: false`; Key Vault is talk-track (config vs secret split) |
| Data isolation | Every query filters `EtpContainerId`, `ClientId`, `ProjectId` |
| Auditability | `AuditTrail` + `AuditActionOccurred` |
| Observability | Serilog, correlation id, `/build-info` (App Insights / Prometheus: talk-track) |
| Maintainability | Central packages, unit tests, Coverlet in CI yaml, Sonar/SAST/Stryker placeholders |
| Deployability | Per-service container + independent k8s Deployment |
| I18n | `Localization:SupportedCultures` — five locales listed, not fully localized UI |
| Data residency | Geography-aware config is a **talking point**; not implemented |

## Documented scale proxies (from the Calc2 write-up)

- ~8–10K users as a rough analogue
- ~35K entities / ~2K engagements
- Seasonal peaks
- Analytics expecting publication on the order of **1–1.5 minutes**

Do not invent `99.99%` availability in an interview.

## Biggest NFR gap (say this)

The repo has **mechanisms**, not **targets**:

- p95 / p99 latency
- requests/sec
- max concurrent users
- RPO / RTO
- availability %
- max calculation duration
- queue processing SLA

Better sentence:

> The architecture contains availability and resilience mechanisms, but this repository does not define a quantified availability SLA. Those targets should be set with performance and capacity tests.
