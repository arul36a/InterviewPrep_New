# Intela Calc

Monorepo scaffold to recreate the Intela / ETP Calc2 platform **phase by phase**.

Every service keeps this call chain:

```
Service  →  Handler  →  Manager  →  Strategy  →  Provider
 (HTTP)     (MediatR)   (use case)  (tax rules)  (data / I/O)
```

| Layer | Allowed to do | Not allowed to do |
| --- | --- | --- |
| **Service** | Map HTTP ↔ commands | Tax math, Mongo, other services |
| **Handler** | Receive a request, call one manager | Rules, SQL, HTTP to other services |
| **Manager** | Orchestrate one use case | Know ASP.NET or MediatR |
| **Strategy** | Pure calculation | I/O |
| **Provider** | Storage, cache, outbound HTTP | Business formulas |

---

## Phases

| Phase | Status | Goal |
| --- | --- | --- |
| [1. One service, sync calc](#phase-1--one-service-sync-calc) | **In the repo** | TaxCalc five-layer vertical slice |
| [2. Tenancy + shared kernel](#phase-2--tenancy--extract-shared-kernel) | Next | Tenant ids + `src/shared` |
| [3. StateMods microservice](#phase-3--statemods-as-a-second-microservice) | Planned | Second host; TaxCalc provider calls it over HTTP |
| [4. ChartOfAccounts](#phase-4--chartofaccounts-reference-data) | Planned | Reference data + 15-minute cache |
| [5. Async jobs](#phase-5--async-jobs-the-real-intela-shape) | Planned | `202` + jobId; JobTracker |
| [6. Calc catalog split](#phase-6--split-the-remaining-calc-catalog) | Planned | AdjustmentCalculation, InternationalCalc, workflow services |
| [7. NFRs + AKS](#phase-7--nfrs--aks) | Planned | Health, retries, KEDA; no invented SLAs |

Do not skip ahead. Each phase adds **one** idea. When a phase is solid, ask for the next one only.

---

## Phase 1 — one service, sync calc

**In the repo now.** One microservice (`TaxCalc`). In-memory store. No jobs, no Mongo, no second service.

```
src/services/TaxCalc/Intela.TaxCalc/
  Services/     HTTP only
  Handlers/     MediatR, calls manager
  Managers/     one use case
  Strategy/     21% federal, no I/O
  Providers/    in-memory store
```

Learn: why five folders beat a fat controller.

```bash
dotnet test Intela.Calc.sln
dotnet run --project src/services/TaxCalc/Intela.TaxCalc
```

Swagger: http://localhost:5101/swagger

```bash
curl -s http://localhost:5101/api/calculations -H 'Content-Type: application/json' -d '{
  "lines": [
    { "accountCode": "4000", "accountName": "Book income", "amount": 100000, "taxTreatment": "Book" },
    { "accountCode": "5000", "accountName": "Meals", "amount": 5000, "taxTreatment": "Permanent" },
    { "accountCode": "6000", "accountName": "Depreciation", "amount": 2000, "taxTreatment": "Temporary" }
  ]
}'
```

Expect taxable **107000**, federal **22470** (21%).

**Stop when you can draw the five folders on a whiteboard without looking.**

---

## Phase 2 — tenancy + extract shared kernel

Still one service. Add `EtpContainerId` / `ClientId` / `ProjectId` on every document. Provider filters by tenant. Move `Models` + provider interface into `src/shared` so the next service can reuse them.

---

## Phase 3 — StateMods as a second microservice

New host, same five folders. TaxCalc **manager** asks a **provider** to call StateMods over HTTP. Strategy in StateMods owns state rates. That is how Intela keeps federal and state independently deployable.

---

## Phase 4 — ChartOfAccounts (reference data)

Third service. TaxCalc provider loads tax treatments (with a 15-minute cache). Calc engines stop hard-coding account meaning.

---

## Phase 5 — async jobs (the real Intela shape)

`POST /calculations` returns **202 + jobId**. Handler does not wait. A subscriber/manager runs the strategy. JobTracker is its own service. This is why TaxCalc is not “a REST API that waits 10 minutes”.

---

## Phase 6 — split the remaining calc catalog

Same template, new hosts: AdjustmentCalculation, InternationalCalc, then orchestration/workflow services. Do not grow TaxCalc into a monolith.

---

## Phase 7 — NFRs + AKS

Health probes, retries, KEDA for seasonal load. Mechanisms first; no invented 99.99% availability SLA.

---

## Angular interview app (separate folder)

UI practice lives in [`angular/`](angular/README.md), not in `docs/`. **Phase 1 only** is coded.

| Phase | Status | Goal |
| --- | --- | --- |
| 1. Standalone + signals | **In the repo** | `App` shell, `Home`, `signal` / `computed`, `@if` / `@for` |
| 2. Components / templates | Next | `input()`, content projection, pipe, directive |
| 3. DI | Planned | Root service, injectors |
| 4. Change detection | Planned | OnPush vs Eager, `effect` |
| 5. Router | Planned | Lazy load, guards, params |
| 6. HTTP + RxJS | Planned | `httpResource`, interceptor, `switchMap` |
| 7. Forms | Planned | Template / Reactive / Signal Forms |
| 8. SSR / a11y / AI | Planned | `@defer`, Aria, MCP vs WebMCP |
| 9. Testing | Planned | TestBed, signal service, harness |

```bash
cd angular
npm install
npm start
```

http://localhost:4200

