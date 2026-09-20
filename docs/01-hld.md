# High-level design (whiteboard)

## System context

```
Users / UI (Angular analogue: src/clients/web)
        │
        ▼
 Azure APIM analogue ──────── Etp.Gateway (YARP)
        │
        ├──────── TaxCalc, StateMods, ChartOfAccounts
        ├──────── JobTracker, AuditTrail
        │
        ├──────── MongoDB (or InMemory repository)
        ├──────── Redis / memory cache
        └──────── Bus (InMemory | Redis pub/sub | Azure Service Bus stub)
                     │
                     ├─ Analytics / Event Hub (not implemented — talk track only)
                     └─ Downstream Corptax / GoSystem (not implemented)
```

## Upstream → Calc → downstream

```
Client trial balance
    → Data Wrangling analogue (POST /api/datasets)
    → PublishedDataset (+ PublishedDatasetData event)
    → TaxCalc / StateMods / reference data / jobs
    → TaxCalculation result
    → CalculationFinished
         ├─ JobTracker
         ├─ AuditTrail
         └─ (talk) Event Hub → Analytics (~1–1.5 min downstream expectation)
```

## Internal service layers (every host, every phase)

```
Service (controller)
    → Handler (MediatR)
        → Manager (use case)
            → Strategy (rules)
            → Provider (data / I/O)
```

Code in the repo today is Phase 1 of that chain only. See [README.md](../README.md).

## Communication

| Style | When | Code |
| --- | --- | --- |
| Sync REST via gateway | Caller needs a response now (create job, read result, apply state mod) | `IInternalHttpClient` + `ServiceDiscovery:*` |
| Async bus | Long-running calc, fan-out to JobTracker/AuditTrail | `IEventPublisher` / `IEventHandler<T>` |
| Stream (talk only) | Analytics | Event Hub — not wired |

## Why AKS + stateless pods

Tax is seasonal. Scale **workers**, not the database, with KEDA CPU (and later queue length). See `deploy/k8s/taxcalc.yaml` (`minReplicaCount: 2`, `maxReplicaCount: 20`, `terminationGracePeriodSeconds: 300`).
