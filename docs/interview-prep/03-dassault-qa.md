# Dassault Systèmes — Lead .NET Interview Q&A

**Bar:** 5–10 years, Lead Software Engineer. ~90–100 minutes.  
**Stack:** C#, .NET Core, SQL Server, React, Azure, design patterns, LINQ. Product-company depth (not only definitions).  
**Videos:** [Part 1](https://www.youtube.com/watch?v=46w-MCOlXZM) · [Part 2](https://www.youtube.com/watch?v=6ItCVMw4KpA) · [Part 3](https://www.youtube.com/watch?v=wIi36JhtWgw)

---

## C# / ASP.NET Core

### 1. `IEnumerable` vs `IQueryable` vs `ICollection` vs `List` — when to use each

| Type | Role |
|---|---|
| **`IEnumerable<T>`** | Sequence you **iterate in memory**. After `ToList()`, further `Where` runs in CLR. |
| **`IQueryable<T>`** | Expression tree. EF sends **filter/sort to SQL** when you materialize. |
| **`ICollection<T>`** | Abstraction that supports **Add/Remove** without exposing `List<T>`. |
| **`List<T>`** | Concrete in-memory collection with **indexing** and list APIs. |

```csharp
// IEnumerable path — ToList() runs SELECT * (plus AsNoTracking here)
IEnumerable<Product> products = db.Products.AsNoTracking().ToList();
foreach (var x in products) { /* iterate in memory */ }

// IQueryable path — query is composed; SQL includes WHERE/ORDER BY at ToList()
IQueryable<Product> query = db.Products.AsQueryable();
var expensive = query.Where(p => p.Price > 100).OrderBy(p => p.Name).ToList();
// SQL: SELECT ... FROM Products WHERE Price > 100 ORDER BY Name
```

**Pros/cons:** `IEnumerable` is simple but can over-fetch. `IQueryable` is right for large data; a sloppy LINQ chain can still generate expensive SQL. `ICollection` hides the concrete type. `List` when you need indexes and a materialized result.

### 2. `ref` vs `out` vs `in`

| | `ref` | `out` | `in` |
|---|---|---|---|
| Caller initializes | Yes | No | Yes |
| Method must assign | No | **Yes** | No |
| Method may modify | **Yes** (read + write) | Writes the result | **No** (read-only by ref) |

- **`ref`:** increment a counter the caller already has.
- **`out`:** Try-parse pattern (`int.TryParse`, `TryGetEmployeeId`).
- **`in`:** pass a large struct by reference without copying, and **forbid** mutation.

Prefer returning a tuple/object in modern C# unless the Try* pattern is the API.

### 3. Explain async/await internally

When execution hits `await` on an incomplete DB/HTTP call, the method **does not block** the request thread. It returns a `Task`. The compiler turns the method into a **state machine**. When the I/O completes, the method resumes after `await`. **`Task` correlates the result back to that request.**

That is why 10,000 concurrent I/O-bound requests do not need 10,000 blocked threads.

**Correct line:** async/await **does not create a new thread** for I/O. It **frees the thread pool thread** while waiting. (If you said “it always spins a child thread,” correct yourself.)

**Cons:** exceptions/cancellation must be handled; `.Result`/`.Wait()` can deadlock; `async` must flow up the call chain.

### 4. Why must you not inject a scoped service into a singleton?

**Captive dependency.** Singleton lives for the app. Scoped (e.g. user context, `DbContext`) should live for one request.

If `ReportService` (singleton) takes `UserService` (scoped) in the constructor, the **first request’s** user/context is captured forever. Request B can see user A’s data.

**Fix:** inject `IServiceScopeFactory` and create a scope **per operation**:

```csharp
public class ReportService
{
    private readonly IServiceScopeFactory _scopes;
    public ReportService(IServiceScopeFactory scopes) => _scopes = scopes;

    public async Task GenerateAsync()
    {
        using var scope = _scopes.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<IUserService>();
        // use users only inside this scope
    }
}
```

**Rule:** a longer-lived service must not capture a shorter-lived one.

### 5. Middleware vs filters vs attributes — and request order

| | What it is | Example |
|---|---|---|
| **Middleware** | Global HTTP pipeline | Exception logging around every request |
| **Filters** | MVC/API **action** pipeline | `IAsyncActionFilter` logging body before/after an action |
| **Attributes** | Metadata | `[HttpGet]`, `[Route]`, `[ApiController]`, `[EnableRateLimiting]`, `[MaxLength]` on properties |

**Order:** HTTP request → **middleware** → routing → **filters** → **controller action**. Attributes do not “run” as a fourth pipeline; they configure routing, auth, validation, rate limits.

Mnemonic: middleware = global request; filter = action; attribute = declaration.

### 6. API rate limiting — threshold and status code

Rate limiting caps how many requests a client may send in a window (e.g. 2 per 30 seconds). Over the limit → **HTTP 429 Too Many Requests**.

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api", o =>
    {
        o.PermitLimit = 2;
        o.Window = TimeSpan.FromSeconds(30);
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

app.UseRateLimiter();

[EnableRateLimiting("api")]
public class ProductsController : ControllerBase { }
```

**How to pick the number:** expected traffic vs API capacity. Anonymous vs logged-in vs premium can have different limits. Protects against bots and accidental storms. Wrong limits block real users; multi-instance apps may need **distributed** counters.

---

## LINQ

### 7. `First` / `FirstOrDefault` / `Single` / `SingleOrDefault`

| | 0 rows | 1 row | 2+ rows |
|---|---|---|---|
| `First` | throws | that row | **first** |
| `FirstOrDefault` | default/`null` | that row | **first** |
| `Single` | throws | that row | **throws** |
| `SingleOrDefault` | default/`null` | that row | **throws** |

`Single` = “this must be unique (email, PK) or the data is wrong.” `First` = “I only need one,” usually after `OrderBy`.

### 8. Second-highest salary (LINQ)

```csharp
var secondHighest = employees
    .Select(e => e.Salary)
    .Distinct()
    .OrderByDescending(s => s)
    .Skip(1)
    .FirstOrDefault();
```

`Distinct` so ties at the top do not hide the next salary.

### 9. Duplicate names (LINQ)

```csharp
var duplicateNames = employees
    .GroupBy(e => e.Name)
    .Where(g => g.Count() > 1)
    .Select(g => g.Key)
    .ToList();
```

---

## React

### 10. `useState`, `useEffect`, `useMemo`, `useCallback`, `useRef`

- **useState** — local state; setter schedules a re-render.
- **useEffect** — side effects (fetch, timers, subscriptions). Return a cleanup function.
- **useMemo** — remember an expensive **value** until deps change (cart total).
- **useCallback** — remember a **function** identity for memoized children.
- **useRef** — mutable box / DOM node **without** re-render (`inputRef.current.focus()`).

Do not memoize every value.

### 11. What happens when React state changes?

`setCount` **schedules** an update. Then: **render** (function runs) → **reconcile** (diff Virtual DOM) → **commit** only the DOM nodes that changed → paint.

Re-render is **not** “throw away the whole DOM.” Count 0→1 updates that text node.

One sentence: *State update schedules a render; React recalculates output, diffs the previous tree, and commits necessary DOM changes.*

### 12. Form validation in React (enterprise)

Do not lead with a pile of `if (!email)`. Use a **schema** — they used **Zod**, often with **React Hook Form**.

```js
const loginSchema = z.object({
  email: z.string().email("Invalid email"),
  password: z.string().min(8, "Minimum 8 characters required"),
});

const result = loginSchema.safeParse(formData);
if (!result.success) {
  // result.error.flatten().fieldErrors
}
```

Why Zod: one schema, typed, reusable on client and (if you want) server.

---

## SQL

### 13. Clustered vs non-clustered index

Same as PwC: clustered = physical order, **one** per table, PK is clustered by default. Non-clustered = separate structure, **many** allowed. Faster reads, extra cost on writes. Non-clustered is **not** “defined as composite”; a composite index is just an index on several columns.

### 14. Second-highest salary (SQL) — two ways

```sql
-- 1) DENSE_RANK
WITH SalaryRank AS (
    SELECT Salary, DENSE_RANK() OVER (ORDER BY Salary DESC) AS SalaryRank
    FROM Employees
)
SELECT Salary FROM SalaryRank WHERE SalaryRank = 2;

-- 2) nested MAX
SELECT MAX(Salary)
FROM Employees
WHERE Salary < (SELECT MAX(Salary) FROM Employees);
```

Ties: `DENSE_RANK` and `< MAX` yield the next **distinct** salary.

### 15. `ROW_NUMBER` vs `RANK` vs `DENSE_RANK`

Window functions number rows **without collapsing** them (`GROUP BY` would collapse).

| | Ties | Next number |
|---|---|---|
| `ROW_NUMBER` | Unique 1,2,3… | Never repeats, never skips |
| `RANK` | Same rank if equal | **Skips** (1, 1, 3) |
| `DENSE_RANK` | Same rank if equal | **No skip** (1, 1, 2) |

Uses: nth salary, top-N, de-dupe, paging.

### 16. What does `PARTITION BY` do?

It **restarts** the window per group. Rows are **not** collapsed. `PARTITION BY Department ORDER BY Salary DESC` → Finance 1..N, then HR starts at 1 again. Contrast with `GROUP BY`, which reduces to one row per group.

### 17. Top two highest-paid employees per department

```sql
WITH EmployeeRanking AS (
    SELECT Name, Department, Salary,
           ROW_NUMBER() OVER (
               PARTITION BY Department ORDER BY Salary DESC
           ) AS RowNum
    FROM Employees
)
SELECT Name, Department, Salary
FROM EmployeeRanking
WHERE RowNum <= 2;
```

Use `RANK`/`DENSE_RANK` if a tie should return more than two rows.

---

## Azure

### 18. Azure App Service vs Azure Functions

| | App Service | Functions |
|---|---|---|
| What | PaaS **host** for the web app / API | **Serverless** code on an **event** |
| Deploy | Whole .NET API, MVC, or microservice | Small functions |
| Example | Host the API | After bulk upload succeeds, send email |

Function triggers to name: **HTTP, timer, queues**, Service Bus, Event Grid.

### 19. Azure Service Bus — why messaging instead of a synchronous API call?

Service Bus is managed **queues/topics**. Producer sends; consumer processes later.

Sync HTTP from React → API that takes 2–5 minutes **blocks the UI**. At Zomato/Amazon volume you cannot hold HTTP open for every order.

Messaging gives: **loose coupling** (order API does not require notification API uptime), **async work**, **load leveling** (10k orders drain at worker speed), **retries + dead-letter** so messages are not dropped.

`async/await` frees threads **inside** one process. A **queue** decouples **services** and absorbs spikes. API can return **202** while fulfillment runs.

### 20. Azure Front Door

Global HTTP(S) **front door**: one URL, WAF, TLS, health probes, route to the healthy origin (India vs US vs UK). The React app does not hard-code region URLs. If India is down, traffic fails over.

---

## Architecture

### 21. Repository pattern (including generic)

Boundary between business logic and data.

**Controller → Service (rules) → Repository (SQL) → DB.** Swap SQL Server for Oracle/Cosmos without rewriting controllers. Tests mock `IRepository`. Register `AddScoped` for service and repo.

Generic `IRepository<T>` for CRUD; add specific methods for real queries.

### 22. Why a repository if EF Core already exists?

**You don’t always need one.** EF already abstracts SQL. A repo that only forwards `ToListAsync()` is ceremony.

Add a repo when access is **complex**, you need a **team/test boundary**, **multiple data sources**, or behavior beyond “call `DbSet`.” Senior answer: **don’t add repository by habit.**

### 23. Microservices and how they talk

Independently deployable services around **business capabilities** (Users, Products, Orders, Payments), often with **their own databases**. Clients (and often service-to-service) go through an **API Gateway**. Prefer **async messaging**; use **sync HTTP/gRPC** only when the caller must have the result now.

### 24. System design: employee task management

**Modules:** org/employees, tasks, projects, comments, notifications, reports, auth.

**Sketch to talk through:**

- React → **Front Door** (WAF, TLS, health, failover)
- **N app instances** (horizontal scale), not one box
- **Azure SQL** (indexes on assignee / due / status; replicas or separate DBs if one DB becomes the funnel) + **cache** for hot reads
- Identity: **Azure AD / Okta**, not a homemade user table as the only IdP
- **Service Bus** for “task assigned” emails so the write API stays fast
- APIs **async**; load balancer / Front Door for unhealthy regions

Talk sequence: requirements → edge + scale-out → data/cache → SSO → queues for notifications → failover.
