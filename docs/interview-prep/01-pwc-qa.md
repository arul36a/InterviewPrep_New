# PwC — .NET Full Stack Interview Q&A

**Bar:** ~70 minutes. Stack: .NET Core APIs, C#, LINQ, EF Core, SQL, React.  
**Videos:** [Overview](https://www.youtube.com/watch?v=_bBSo415L0I) · [Part 1 SQL](https://www.youtube.com/watch?v=-mfrRJ9aT2w) · [Part 2 .NET](https://www.youtube.com/watch?v=JTm9nrJVcwI) · [Part 3 C#](https://www.youtube.com/watch?v=0nvTq2mw8a8) · [Part 4 LINQ/EF](https://www.youtube.com/watch?v=0Zova0qmb1I) · [Part 5 React](https://www.youtube.com/watch?v=aTQKdCuDxVk)

They started with SQL, then .NET, C#, LINQ/EF, then React.

---

## SQL

### 1. Clustered vs non-clustered index? When do you use each?

A **clustered index** is the table’s physical sort order. Rows are stored in the order of the indexed column. SQL Server allows **one clustered index per table** (usually the primary key).

A **non-clustered index** is a separate B-tree: indexed column(s) plus a pointer (row locator) back to the real row. Table order does not change. You can have **many** non-clustered indexes.

**When clustered:** range/lookup on the column that defines row order — `WHERE Id BETWEEN 1 AND 100`, `WHERE Id = @id`. Demo they used: insert IDs `3, 1, 2`, then `CREATE CLUSTERED INDEX IX_students_id ON students(id)` — `SELECT *` then returns 1, 2, 3.

**When non-clustered:** frequent filters/joins on columns that are **not** the clustered key — name, email, category.

```sql
CREATE CLUSTERED INDEX IX_students_id ON students(id);
CREATE NONCLUSTERED INDEX IX_students_name ON students(name);
SELECT * FROM students WHERE name = 'Amit'; -- uses NCI on name
```

**Say this:** “Only one clustered index per table. Non-clustered stores a copy of the key plus a pointer, separately from the heap/clustered data.”

### 2. What is a CTE vs a temp table? When do you use each?

**CTE (Common Table Expression):** a named, temporary result set that exists **only for that statement**. After the query finishes, it is gone.

```sql
WITH EmployeeCte AS (
    SELECT * FROM Employees WHERE Salary < 90000
)
SELECT * FROM EmployeeCte;
-- SELECT * FROM EmployeeCte;  -- Invalid object name
```

**Temp table:** a real table in **`tempdb`** (system databases → Temporary Tables). Local temps start with `#`. Reusable for the **session**. Dropped when the session ends (close SSMS / disconnect).

```sql
SELECT EmployeeId, Name, DepartmentId, Salary
INTO #HighSalaryEmployees
FROM Employees
WHERE Salary > 90000;

SELECT COUNT(*) FROM #HighSalaryEmployees;
SELECT AVG(Salary) FROM #HighSalaryEmployees;
SELECT * FROM #HighSalaryEmployees WHERE DepartmentId = 1;
```

**Interview follow-up they asked:** *Where is a temp table stored?* → **`tempdb`**, not your application database.

| Use CTE | Use temp table |
|---|---|
| One-shot reporting query | Same intermediate set used **several times** |
| Ranking / filter / shape data once | Large sets, ETL, monthly sales jobs |
| Readability vs nested subqueries | Need **indexes** on the intermediate set |

Example CTE scenario: top 3 salaries per department in a single report query.

### 3. Write SQL for the top two salary holders per department

**Preferred (window function):**

```sql
SELECT EmployeeName, DepartmentId, Salary
FROM (
    SELECT
        EmployeeName,
        DepartmentId,
        Salary,
        ROW_NUMBER() OVER (
            PARTITION BY DepartmentId
            ORDER BY Salary DESC
        ) AS RankNo
    FROM Employees
) t
WHERE RankNo <= 2;
```

If ties should all come back, use `DENSE_RANK()` instead of `ROW_NUMBER()`.

A correlated `COUNT` of higher salaries without `PARTITION BY` gives **top 2 overall**, not per department. If they say “per department,” the window function is the answer they want.

### 4. Find the Nth highest salary

```sql
SELECT Salary
FROM (
    SELECT Salary,
           DENSE_RANK() OVER (ORDER BY Salary DESC) AS rnk
    FROM Employees
) t
WHERE rnk = 3; -- third highest; change N as needed
```

`DENSE_RANK` keeps the next distinct salary as 2 after a tie. `RANK` would skip numbers (1, 1, 3).

### 5. How do you improve stored-procedure performance?

1. **Correct indexes** (clustered + targeted non-clustered).
2. **Do not `SELECT *`** — list only needed columns.
3. **`SET NOCOUNT ON`** at the start — skips `N rows affected` traffic. (Use it; do not skip it.)
4. Prefer **joins** over correlated subqueries unless the subquery is clearly cheaper.
5. **Avoid wrapping columns in functions in `WHERE`** (`YEAR(CreatedAt) = 2026` kills index use).
6. **Temp tables only when needed** — large reuse, extra indexes on the work set. Do not temp-table every query.

---

## .NET / ASP.NET Core

### 6. Implement custom middleware, step by step

Convention: field + constructor + `InvokeAsync(HttpContext)` + register with `UseMiddleware<T>()`. Typical example: **global exception handling**. Also name auth, HTTPS redirect, and logging as other uses.

```csharp
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred." });
        }
    }
}

// Program.cs — register early so it wraps the rest of the pipeline
app.UseMiddleware<ExceptionMiddleware>();
```

Do **not** register middleware with `builder.Services.Add...` as the primary step. That is DI; pipeline registration is `app.UseMiddleware<T>()`.

### 7. What is `RequestDelegate`?

A delegate for **the next middleware** in the pipeline:

```csharp
public delegate Task RequestDelegate(HttpContext context);
```

You store `_next` and `await _next(context)` to continue. If you skip the call, you **short-circuit**.

### 8. Explain JWT in detail

JWT is a compact, **stateless** token for authentication/authorization. The server does **not** store session data; the client sends `Authorization: Bearer <token>` on each call. Used for REST APIs, microservices, mobile.

**Three parts, dotted:** `header.payload.signature`

| Part | Contents |
|---|---|
| Header | Type (`JWT`) + algorithm (`HS256` / `RS256`) |
| Payload | Claims: user id, name, roles, `exp`, issuer |
| Signature | Integrity: hash of header + payload + **secret / private key** |

Flow: login → issue signed token → client stores it → API validates signature, expiry, issuer/audience → authorize by role/policy.

*(If a slide says “stateful,” ignore it. The explanation is classic **stateless** auth.)*

### 9. Service lifetimes — which did you use?

| Lifetime | Instance | Typical use |
|---|---|---|
| **Singleton** | One for the app | Config, cache, logging |
| **Scoped** | One per HTTP request | **`DbContext`**, app/business services |
| **Transient** | New on every resolve | Stateless helpers, string/SQL utilities |

They said they used **scoped and transient** most. The line interviewers wait for: **`DbContext` is scoped**.

### 10. How does async/await work behind the scenes? Why is it faster?

**`async`/`await` does not create a new thread.** On `await` of an incomplete task:

1. The compiler-built **state machine** pauses the method.
2. The **thread is returned to the thread pool**.
3. I/O continues in the background.
4. On completion, execution **resumes**; **`Task`** delivers the result to the right caller.

Demo: three sync sleeps of 3s + 4s + 7s ≈ **14s**. Same delays with `Task.Delay` + `Task.WhenAll` ≈ **7s** (longest wait). Faster because waits **overlap**, not because CPU math is quicker.

Do not use `.Result` / `.Wait()` on async code (deadlock risk).

### 11. Redirect HTTP to HTTPS?

Yes:

```csharp
app.UseHttpsRedirection();
if (!app.Environment.IsDevelopment())
    app.UseHsts();
```

Needs a valid certificate and HTTPS on the host (Kestrel / IIS / Azure). You *could* write custom middleware; the interview answer is the built-in one.

### 12. Task vs Thread vs ValueTask

- **Process** ≈ factory. **Thread** ≈ worker (expensive to create/block).
- **Task** ≈ job assigned to the thread pool. Abstraction used by async/await; can return a result.
- **ValueTask** ≈ like Task but a **struct**. Use when the result is **often already ready** (cache hit) to skip a `Task` allocation. Do not await the same `ValueTask` twice.

### 13. Global exception handling?

Same as Q6: exception middleware around `_next`, log, return a consistent payload. Flow: request → middleware wraps pipeline → controller throws → catch in middleware. Do not rely only on per-action `try/catch`.

---

## C#

### 14. Count occurrences of each character

Use `Dictionary<char, int>`. They counted every character, including spaces.

```csharp
string str = "net interview hub";
var count = new Dictionary<char, int>();

foreach (char c in str)
{
    count.TryGetValue(c, out int n);
    count[c] = n + 1;
}

foreach (var item in count)
    Console.WriteLine($"{item.Key}:{item.Value}");
```

### 15. Remove duplicate characters from an array

Use `HashSet<T>` — `Add` ignores duplicates.

```csharp
char[] array = { 'a', 'b', 'c', 'a', 'b', 'e' };
var unique = new HashSet<char>(array); // a, b, c, e
```

If order must be preserved exactly, walk once and add to a `List` only when `HashSet.Add` returns true.

### 16. SOLID — real project examples (they rejected banking)

Use **report generation**: generate, save (blob/disk), email, formats PDF / Excel / CSV.

**SRP — one reason to change.** Do not put generate + save + email on one `Report` class. Split `ReportGenerator`, `ReportSaver`, `ReportEmailer`.

**OCP — open for extension, closed for modification.** No `if (type == "PDF")`. `IReport { void Generate(); }` + `PdfReport` / `ExcelReport`. New format = new class.

**LSP — subtypes must be substitutable.** Every `IReport` must actually generate. A `CsvReport` that throws `NotImplementedException` violates LSP.

**ISP — no fat interfaces.** Do not force PDF to implement `SendEmail()`. Split `IReportGenerator` and `IEmailSender`.

**DIP — depend on abstractions.** `ReportService` takes `IReportGenerator` in the constructor, not `new PdfReport()`. Enables tests and swapping Excel/PDF via DI.

They said DIP via constructor injection is everyday enterprise .NET.

### 17. Call a third-party API. Why not `new HttpClient()`?

`HttpClient` (GET/POST/…). Demo: JSONPlaceholder `/users`, deserialize with case-insensitive JSON.

**Problem:** `new HttpClient()` per request exhausts sockets (`TIME_WAIT`). A long-lived static client is better but still weak on DNS refresh, named clients, retries, and tests.

**Answer:** **`IHttpClientFactory` / typed client:**

```csharp
builder.Services.AddHttpClient<UserService>(client =>
{
    client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
});
```

### 18. Why an interface in DI instead of a concrete class?

If `OrderService` does `new EmailService()`, it is tightly coupled. An `INotificationSender` lets you inject email/SMS and mock in tests. That **is** DIP, not “because DI requires interfaces.”

### 19. Implicit vs explicit cast

- **Implicit:** no data loss, automatic (`int` → `double`).
- **Explicit:** possible loss, you write `(int)99.99` → `99`.

Payroll/shopping rounding is a fine spoken example. Mapping an EF entity to a slim DTO is **mapping**, not a language cast — still a good “don’t leak salary/IsDeleted to the UI” story.

### 20. Extension methods — including an extra parameter

Static class, static method, first parameter `this Type`.

```csharp
public static class StringExtensions
{
    public static string ToProperCase(this string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return char.ToUpper(input[0]) + input[1..].ToLower();
    }

    public static string ToProperCase(this string input, string suffix)
        => $"{input.ToProperCase()} {suffix}";
}

"net interview hub".ToProperCase();           // "Net interview hub"
"net interview hub".ToProperCase("channel");  // extra arg after `this`
```

Other use they named: date → ISO string.

### 21. Design patterns — types and which to prepare

| Family | About | Name these |
|---|---|---|
| Creational | Object creation | Singleton, Factory, Abstract Factory, Builder |
| Structural | Composition | Adapter, Decorator, Facade, Proxy |
| Behavioral | Communication | Observer, Strategy, Command, Chain of Responsibility |

Be ready to **explain two you used**: Repository, Factory, Abstract Factory, CQRS. Do not recite the whole GoF list.

---

## LINQ and EF Core

### 22. `IEnumerable` vs `IQueryable` — which is faster?

**`IQueryable`:** expression tree; provider (EF) translates `Where`/`OrderBy` to SQL. Filter runs **in the database**. Faster for large tables.

**`IEnumerable`:** in-memory. `context.Employees.ToList()` then `.Where(...)` loads **the whole table** first.

Analogy they used: IEnumerable = bring the warehouse to the office, then search. IQueryable = ask the warehouse for only the needed items.

```csharp
// IQueryable — SQL: WHERE Salary > 50000
var query = _context.Employees.Where(e => e.Salary > 50000);
var result = query.ToList(); // executes here

// IEnumerable — SQL: SELECT * then filter in CLR
IEnumerable<Employee> data = _context.Employees.ToList();
var filtered = data.Where(e => e.Salary > 50000);
```

### 23. What are the EF migration commands?

Install tools: `dotnet tool install --global dotnet-ef`. Keep EF Core packages on the **same version**.

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet ef migrations remove   # undo last unapplied migration
```

(`database update`, not `update database`, if you are on the `dotnet ef` CLI. Package Manager Console still uses `Update-Database`.)

### 24. How does SQL know a migration ran?

EF creates **`__EFMigrationsHistory`** in that database. Each applied migration is a row. That is the source of truth for “what already ran.”

### 25. Immediate vs deferred execution. Return type without `ToList`?

LINQ to EF **builds** a query until a materializing call: `ToList`, `ToArray`, `First`, `Count`, `foreach`, etc.

If you write `_context.Employees.Where(...)` and **do not** materialize, the type is **`IQueryable<Employee>`**, not `List` / `IEnumerable` of already-loaded rows.

### 26. LINQ: top two salaries per department

```csharp
var result = _context.Employees
    .GroupBy(e => e.Department)
    .Select(g => new
    {
        Department = g.Key,
        TopEmployees = g.OrderByDescending(e => e.Salary).Take(2).ToList()
    })
    .ToList();
```

---

## React / JavaScript

### 27. What are React hooks? Explain them.

Hooks are functions that let **function components** use state and lifecycle without classes. Name: `useState`, `useEffect`, `useContext`, `useMemo`, `useCallback`, `useRef`, `useReducer`.

`useState` example: `const [count, setCount] = useState(0)` — button increments, label shows count.

### 28. How does `useState` work with an empty vs filled array?

Initial value can be `[]` or `[{ ... }]`. React re-renders when you call the setter with a **new reference**. Mutating the same array in place does not notify React. Always `setUsers([...users, next])` or a new array.

### 29. `useMemo` vs `useCallback` — real use cases

| | `useMemo` | `useCallback` |
|---|---|---|
| Caches | A **value** (calculation) | A **function** |
| Recalc when | Dependency array changes | Dependency array changes |
| Use | Filter/sort large lists, expensive math | Pass a stable callback to `React.memo` children |

Do not wrap everything.

### 30. Write a counter with increment and decrement

```jsx
import { useState } from "react";

export default function Counter() {
  const [count, setCount] = useState(0);
  return (
    <div>
      <h2>Count: {count}</h2>
      <button onClick={() => setCount(count + 1)}>Increment</button>
      <button onClick={() => setCount(count - 1)}>Decrement</button>
    </div>
  );
}
```

### 31. `let a = 10; let b = "20";` log `a+b`, `a-b`, `a*b`

| Expression | Result | Why |
|---|---|---|
| `a + b` | `"1020"` | `+` concatenates if either side is a string |
| `a - b` | `-10` | `-` coerces `"20"` to number |
| `a * b` | `200` | `*` also coerces to number |
