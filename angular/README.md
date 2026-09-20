# Angular interview app

Separate from the .NET `Intela.Calc` solution. **Phase 1 is in this folder.** Later phases are a map only — ask for the next phase when the current one is solid.

Angular **21** (CLI on this machine). Node 24.3 cannot run Angular **22** CLI (needs Node 24.15+). The APIs you practice here are the ones interviewers want; v22 mainly made Signal Forms / `httpResource` / Aria **stable** and OnPush the default.

```bash
cd angular
npm install
npm start
```

http://localhost:4200

---

## Call chain (UI analogue of TaxCalc)

```
Route / component  →  signals + computed  →  template (@if / @for)
```

---

## Phases

| Phase | Status | Goal |
| --- | --- | --- |
| [1. Standalone + signals](#phase-1--standalone-shell--signals) | **In the repo** | Shell, `Home`, `signal` / `computed`, `@if` / `@for` |
| [2. Components / templates](#phase-2--components-templates-lifecycle) | Next | `input()`, content projection, pipe, directive |
| [3. DI](#phase-3--di) | Planned | Root service, injectors |
| [4. Change detection](#phase-4--change-detection-onpush--eager--effect) | Planned | OnPush vs Eager, `effect` |
| [5. Router](#phase-5--router) | Planned | Lazy load, guards, params |
| [6. HTTP + RxJS](#phase-6--http--rxjs) | Planned | `httpResource`, interceptor, `switchMap` |
| [7. Forms](#phase-7--forms) | Planned | Template / Reactive / Signal Forms |
| [8. SSR / a11y / AI](#phase-8--ssr-perf-a11y-ai-talk--light-code) | Planned | `@defer`, Aria, MCP vs WebMCP |
| [9. Testing](#phase-9--testing) | Planned | TestBed, signal service, harness |

Do not skip ahead. Each phase adds **one** cluster of interview questions. When a phase is solid, ask for the next one only.

---

## Phases (detail — cover every interview question)

Do not skip ahead. Each phase adds **one** cluster of questions.

### Phase 1 — standalone shell + signals  ← you are here

**In the repo:** `App` shell, `Home` with `signal` / `computed`, `@if`, `@for` + `track`, zoneless CD.

Questions: standalone vs NgModule · data binding · `inject()` vs constructor · signals vs Observables (intro) · `signal` / `computed` · `@if` / `@for` / `track` · zoneless intro

**Stop when** you can explain why `count()` is a function and why `@for` needs `track`.

---

### Phase 2 — components, templates, lifecycle

Add a child card, `input()` / `output()`, `ng-content`, a tiny attribute directive, a pure pipe, `@let`, `host`.

Questions: component vs directive vs pipe · lifecycle · view vs content · `viewChild` · structural vs attribute directives · pure vs impure pipes · `input`/`output`/`model` · host bindings · AOT/Ivy (talk)

---

### Phase 3 — DI

Add `@Service()` (or `providedIn: 'root'`) tax-label store, route-level provider, `inject()`.

Questions: DI tree · root vs component vs route providers · hierarchical injectors · `@Service()` vs `@Injectable`

---

### Phase 4 — change detection (OnPush / Eager / effect)

Show a legacy Eager-style mutation vs a signal update. Add `effect` + `untracked`.

Questions: Zone.js vs zoneless · OnPush vs Eager · signals in OnPush templates · `effect` / `linkedSignal` / `untracked`

---

### Phase 5 — router

Lazy `loadComponent` for a Calc page, `CanActivate` / `CanMatch`, `withComponentInputBinding`, parent params.

Questions: lazy load · guards · resolver vs fetch-in-page · route params / parent inheritance

---

### Phase 6 — HTTP + RxJS

`HttpClient` POST vs `httpResource` GET, functional interceptor, `switchMap` typeahead, `takeUntilDestroyed`.

Questions: `HttpClient` vs `resource` / `httpResource` / `rxResource` · interceptors · mapping operators · Subjects · unsubscribe · `async` pipe

---

### Phase 7 — forms

One screen: template-driven, one Reactive Form, one Signal Form (`form()` / `[formField]`).

Questions: three form styles · Signal Forms API · CVA · error display / disable submit

---

### Phase 8 — SSR, perf, a11y, AI (talk + light code)

`NgOptimizedImage`, `@defer (on viewport)`, notes on hydration / Aria / MCP vs WebMCP / migration order.

Questions: CSR/SSR/hydration · `NgOptimizedImage` · Aria vs Material · MCP vs WebMCP · migrate NgModule+Zone

---

### Phase 9 — testing

Harness or TestBed for Home + a signal service. Component harness note.

Questions: unit-test standalone · test signal service · component harness

---

When Phase 1 is solid, ask for **Phase 2** only.
