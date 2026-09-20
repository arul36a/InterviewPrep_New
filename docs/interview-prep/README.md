# .NET Full Stack Interview Prep

Study notes from the YouTube playlist **[Tech Interview Experiences | .NET Full Stack](https://www.youtube.com/playlist?list=PLuJfxVbOQwAgXfdUbOdyLtCUUWMomUNom)** (DotNet Interview Hub). Answers are rewritten for interview use — not a transcript dump.

| Company | Role / bar | Duration | Videos |
|---|---|---|---|
| **PwC** | .NET Full Stack | ~70 min technical | Parts 1–5 + overview |
| **Barclays** | Senior Software Engineer (5+) | ~1.2 hr, all technical | Parts 1–2 |
| **Dassault Systèmes** | Lead Software Engineer (5–10 yrs) | ~90–100 min | Parts 1–3 |

Stack across the playlist: **C#, ASP.NET Core Web API, LINQ, EF Core, SQL Server, React, Azure, design patterns**.

## Files

- [01-pwc-qa.md](01-pwc-qa.md) — SQL, .NET, C#, LINQ/EF, React
- [02-barclays-qa.md](02-barclays-qa.md) — OOP output questions, SOLID/DI, LINQ/SQL, EF, collections, middleware
- [03-dassault-qa.md](03-dassault-qa.md) — senior .NET, React, SQL windows, Azure, microservices, system design

## Topic map (every topic that appeared)

### SQL Server
- Clustered vs non-clustered indexes (and when to use each)
- CTE vs temp table (and where temp tables live: `tempdb`)
- Stored-procedure performance
- Nth highest salary (`DENSE_RANK`, nested `MAX`)
- Top N per department (`ROW_NUMBER` + `PARTITION BY`)
- `ROW_NUMBER` vs `RANK` vs `DENSE_RANK`
- Duplicate emails (`GROUP BY` + `HAVING`)

### C# / OOP
- Method hiding (`new`) vs overriding (`virtual`/`override`)
- `ref` / `out` / `in`
- Implicit vs explicit casting
- Extension methods (including extra parameters)
- `class` vs `record`
- `Dictionary` vs `ConcurrentDictionary`
- `Task` vs `Thread` vs `ValueTask`
- Iterator pattern (`IEnumerable` / `IEnumerator`)
- Character-count and duplicate-removal programs
- Run-length encoding (`aaabbccccd` → `a3b2c4d1`)

### ASP.NET Core / .NET
- Custom middleware + `RequestDelegate`
- Middleware vs filters vs attributes
- Middleware order; AuthN before AuthZ
- Chain of Responsibility
- DI lifetimes (Singleton / Scoped / Transient)
- Captive dependency: scoped inside singleton
- JWT (header.payload.signature)
- HTTP → HTTPS (`UseHttpsRedirection`)
- Global exception handling
- API versioning (URL, query string, header)
- API rate limiting (HTTP 429)
- `HttpClient` vs `IHttpClientFactory`
- Why inject interfaces, not concretions
- async/await internals (does **not** spawn a thread)

### LINQ / EF Core
- `IEnumerable` vs `IQueryable` vs `ICollection` vs `List`
- Deferred vs immediate execution
- `First` / `FirstOrDefault` / `Single` / `SingleOrDefault`
- `ToList()` vs `AsNoTracking()`
- EF migrations (`dotnet ef migrations add`, `dotnet ef database update`)
- `__EFMigrationsHistory`
- LINQ: duplicates, second-highest salary, top 2 per group

### React / JavaScript
- Hooks: `useState`, `useEffect`, `useMemo`, `useCallback`, `useRef`, `useContext`, `useReducer`
- `useState` with `[]` vs a filled array (new reference → re-render)
- Counter component
- Render cycle: schedule → render → reconcile → commit
- Form validation with Zod (+ React Hook Form)
- JS coercion: `"20"` with `+`, `-`, `*`

### Azure / architecture
- App Service vs Functions
- Service Bus vs synchronous HTTP
- Front Door
- Repository pattern (and when EF already is enough)
- Microservices + API gateway + async messaging
- Employee task-management system design
- SOLID (report-generation examples, not banking)
- Why SOLID if DI already exists
- Multiple implementations of one interface (factory)
- LSP with Bird/Ostrich
- Design-pattern families (creational / structural / behavioral)

## Questions that repeat across companies

Memorize these; they showed up more than once:

1. Clustered vs non-clustered index
2. `IEnumerable` vs `IQueryable`
3. Top 2 salaries per department
4. Second / Nth highest salary
5. Duplicate emails or names (`GroupBy` + count > 1)
6. async/await internals
7. Middleware pipeline
8. SOLID + DIP / DI
9. React `useMemo` vs `useCallback`
