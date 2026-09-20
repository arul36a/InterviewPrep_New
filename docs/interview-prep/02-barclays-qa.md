# Barclays — Senior .NET Interview Q&A

**Bar:** 5+ years, Senior Software Engineer. ~1.2 hours, fully technical.  
**Topics they listed:** C# OOP, SOLID, LINQ, SQL, EF Core.  
**Videos:** [Part 1](https://www.youtube.com/watch?v=8IiC-lUfo54) · [Part 2](https://www.youtube.com/watch?v=PuY4ZesTGRo)

---

### 1. What is the output? (method hiding with `new`)

```csharp
class Base
{
    public void Print() => Console.WriteLine("Base");
}

class Derived : Base
{
    public new void Print() => Console.WriteLine("Derived");
}

Base obj = new Derived();
obj.Print(); // "Base"
```

`new` **hides** the parent method. There is no `virtual`, so binding is **compile-time, by reference type**. Reference is `Base` → Base.Print.

```csharp
Derived d = new Derived();
d.Print(); // "Derived" — reference type is Derived
```

**Rule:** method hiding → **reference type**, not object type.

**Use case:** old `Report.Generate()` library; `AdvancedReport` hides `Generate` with `new`. Code that still types the variable as `Report` keeps old behavior (backward compatible). New code must type `AdvancedReport` to get the new method.

### 2. What is the output? (virtual + override)

```csharp
class Base
{
    public virtual void Print() => Console.WriteLine("Base");
}

class Derived : Base
{
    public override void Print() => Console.WriteLine("Derived");
}

Base obj = new Derived();
obj.Print(); // "Derived"
```

**Runtime polymorphism.** CLR uses the **object type** (`Derived`) even though the reference is `Base`.

**Remember:** hiding → reference type. Overriding → object type.

### 3. Different ways to version an API

Three standard approaches:

1. **URL:** `/api/v1/orders`, `/api/v2/orders` + route attributes.
2. **Query string:** `/api/orders?api-version=1.0`
3. **Header:** `api-version: 1.0` (media-type versioning is a fourth, less often required).

For a senior answer: install the versioning NuGet, set default version on the builder, mention that enterprises often pick **URL or header** because clients stay explicit.

### 4. Why SOLID if we already have dependency injection?

DI is **not** SOLID. DI is **one way to implement DIP** (the D). It makes classes loosely coupled. SOLID as a whole is about **maintainable, scalable, extensible** design (SRP, OCP, LSP, ISP, plus DIP).

```csharp
// Bad — even if you later “use DI,” this new is tight coupling
public class OrderService
{
    private readonly SqlRepository _repo = new SqlRepository();
}

// Better — DIP via constructor injection
public class OrderService
{
    private readonly IRepository _repo;
    public OrderService(IRepository repo) => _repo = repo;
}
```

**Line they want:** “DI alone does not guarantee a SOLID design.”

### 5. Which SOLID principle does this violate? (Bird / Ostrich)

```csharp
class Bird
{
    public virtual void Fly() { /* fly */ }
}

class Ostrich : Bird
{
    public override void Fly() => throw new Exception("Can't fly");
}

Bird b = new Ostrich();
b.Fly(); // blows up
```

**Liskov Substitution Principle.** A child must be usable wherever the parent is expected, without breaking behavior. Ostrich cannot substitute Bird if Bird promises `Fly()`.

Better: `IFlyable` only on birds that fly; Ostrich is a `Bird` without `Fly()`.

(Open/Closed is “open for extension, closed for modification” — don’t mix it up here.)

### 6. Multiple implementations of the same interface in DI?

`IPaymentService` → CreditCard, DebitCard, UPI. Resolve at runtime with a **factory** + `IServiceProvider` (or keyed services in newer .NET).

```csharp
public class PaymentFactory
{
    private readonly IServiceProvider _services;
    public PaymentFactory(IServiceProvider services) => _services = services;

    public IPaymentService Get(string type) => type switch
    {
        "credit" => _services.GetRequiredService<CreditCardPayment>(),
        "debit"  => _services.GetRequiredService<DebitCardPayment>(),
        "upi"    => _services.GetRequiredService<UpiPayment>(),
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
}
```

Register each implementation; factory picks by type string from the request.

### 7. LINQ and SQL: duplicate email IDs

```csharp
var duplicateEmails = users
    .GroupBy(u => u.Email)
    .Where(g => g.Count() > 1)
    .Select(g => g.Key);
```

```sql
SELECT EmailId
FROM Employees
GROUP BY EmailId
HAVING COUNT(*) > 1;
```

They wanted **both**. Mention an index on email for large tables.

### 8. Compress consecutive characters (`aaabbccccd` → `a3b2c4d1`)

Single pass + `StringBuilder`. Track current char and count; on change, append and reset; after the loop, append the last run.

```csharp
static string CompressRuns(string input)
{
    if (string.IsNullOrEmpty(input)) return input;

    var result = new StringBuilder();
    char current = input[0];
    int count = 1;

    for (int i = 1; i < input.Length; i++)
    {
        if (input[i] == current) count++;
        else
        {
            result.Append(current).Append(count);
            current = input[i];
            count = 1;
        }
    }

    result.Append(current).Append(count);
    return result.ToString();
}
```

Watch the index: compare `input[i]`, not `input[0]`. Handle `""` and `"a"` → `"a1"`.

### 9. `ToList()` vs `AsNoTracking()`

They are **not alternatives**. Often used together.

- **`ToList()` / `ToListAsync()`:** execute now, materialize a `List<T>`.
- **`AsNoTracking()`:** skip the change tracker. Reads won’t be persisted by `SaveChanges` unless you attach them.

Tracked queries are for updates. GET/report/dashboard: `AsNoTracking()`.

```csharp
return await _db.Users.AsNoTracking().Where(u => u.IsActive).ToListAsync();

var user = await _db.Users.FirstAsync(u => u.Id == id); // tracked
user.Name = name;
await _db.SaveChangesAsync();
```

### 10. `Dictionary` vs `ConcurrentDictionary`

`Dictionary` is **not thread-safe**. Concurrent reads+writes can throw or corrupt. `ConcurrentDictionary` is safe for concurrent collection operations (`TryAdd`, `GetOrAdd`, `AddOrUpdate`) without your own lock.

- Dictionary: single-threaded in-memory catalog.
- ConcurrentDictionary: session cache, background workers, ASP.NET shared mutable maps.

Collection safety ≠ business atomicity (a money transfer still needs a transaction).

### 11. `class` vs `record` (C# 9+)

Records are reference types meant for **data**. Classes compare **identity**; records compare **value**.

```csharp
public class PersonClass { public int Id { get; set; } public string Name { get; set; } }
public record PersonRecord(int Id, string Name);

new PersonClass { Id = 1, Name = "Alex" }
    == new PersonClass { Id = 1, Name = "Alex" }; // false

new PersonRecord(1, "Alex") == new PersonRecord(1, "Alex"); // true
```

**Records:** DTOs, API contracts, events, immutable messages. **Classes:** entities/services with identity and behavior. Don’t call `record` a value type unless it is `record struct`.

### 12. Which design pattern is ASP.NET Core middleware?

**Chain of Responsibility.** Each link handles `HttpContext` and either calls `await _next(context)` or short-circuits.

### 13. How does middleware work internally?

Request walks **registration order** (M1 → M2 → M3 → endpoint). Response walks **reverse** (M3 → M2 → M1). Code **before** `await next` is inbound; **after** is outbound. `app.UseX()` order **is** the pipeline.

### 14. Recommended middleware order

1. Exception handling (first — must wrap everything)
2. HTTPS redirection
3. Static files
4. Routing
5. Authentication
6. Authorization
7. Map controllers / endpoints

```csharp
app.UseExceptionHandler("/error");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

They treated “exception middleware first” as a discriminator.

### 15. Why authentication before authorization?

Authentication = **who**. Authorization = **are they allowed**. AuthZ needs an identity; swapping the order breaks the model.

### 16. Iterator pattern

Sequential access without exposing internals. In C# that is `IEnumerable` / `IEnumerator`. `foreach` → `GetEnumerator()`, `MoveNext()`, `Current`. Lists, dictionaries, queues, LINQ all sit on this.
