# Rule 03: .NET 10 Coding Standards & Conventions

**Scope**: All C# files across every layer of the solution.  
**Enforcement**: High — consistency is critical for long-term maintainability.

---

## 1. C# Language Features

### File-Scoped Namespaces
Always use file-scoped namespaces (C# 10+). Never use the old block-style namespace with braces.

```csharp
// ✅ Correct
namespace Crm.Domain.Entities;

// ❌ Wrong
namespace Crm.Domain.Entities
{
    ...
}
```

### Records for DTOs, Commands, and Queries
Use `record` types for all immutable data transfer objects: DTOs, Commands, and Queries. Records give you value equality, `with` expressions, and a cleaner API surface.

```csharp
// ✅ Correct — use record for DTOs, Commands, Queries
public record CustomerDto : BaseAuditableDto<long>
{
    public string Name { get; init; } = string.Empty;
}

public record CreateCustomerCommand : IRequest<Result<long>>
{
    public string Name { get; init; } = string.Empty;
}

// ❌ Wrong — don't use class for DTOs/Commands unless there is a specific reason
public class CustomerDto { ... }
```

### `init` Properties
Use `init` instead of `set` on properties inside DTOs, Commands, and Queries. This enforces immutability after construction.

```csharp
public string Name { get; init; } = string.Empty;
```

### `new()` Collection Initializers
Prefer the target-typed `new()` for collection and object initialization.

```csharp
// ✅ Correct
public IList<Task> Tasks { get; private set; } = [];   // C# 12 collection expression
List<string> names = new();                              // target-typed new

// ❌ Avoid
public IList<Task> Tasks { get; private set; } = new List<Task>();
```

### Pattern Matching and Null Checks
Prefer `is null` / `is not null` over `== null` / `!= null`.

```csharp
// ✅ Correct
if (entity is null) throw new NotFoundException(...);
if (customer is not null) { ... }

// ❌ Wrong
if (entity == null) { ... }
```

---

## 2. Naming Conventions

| Element | Convention | Example |
|---|---|---|
| Classes / Records | PascalCase | `CustomerDto`, `CreateCustomerCommand` |
| Interfaces | `I` prefix + PascalCase | `IApplicationDbContext` |
| Private fields | `_camelCase` | `_context`, `_authService` |
| Parameters / locals | camelCase | `entityId`, `command` |
| Constants | UPPER_SNAKE_CASE | `MAX_PAGE_SIZE` |
| Async methods | Suffix `Async` | `GetCustomerAsync()` |
| Validators | `<CommandName>Validator` | `CreateCustomerCommandValidator` |
| Handlers | `<QueryOrCommand>Handler` | `GetCustomerQueryHandler` |
| Feature files | `<Entity>Features.cs` | `CustomerFeatures.cs` |
| Controllers | Plural + `Controller` | `CustomersController.cs` |

---

## 3. String Handling

- Prefer `string.Empty` over `""` for empty string assignments.
- Always initialize non-nullable string properties: `public string Name { get; set; } = string.Empty;`
- Do **not** leave a `string` property without `?` or `= string.Empty` — the project has `<Nullable>enable</Nullable>` enabled.

```csharp
// ✅ Correct
public string Name { get; set; } = string.Empty;
public string? Description { get; set; }    // nullable — intentional

// ❌ Wrong — will trigger nullable warnings
public string Name { get; set; }
```

---

## 4. Commenting & Documentation

### XML Doc Comments
Write `///` XML documentation comments on:
- All `public` interfaces (especially in `Application/Common/Interfaces/`).
- All MediatR Handlers (explain the *why*, not just what they do).
- Non-obvious public methods.

Explain **why** the code exists, not what it mechanically does:
```csharp
/// <summary>
/// Validates that a customer name is unique within the system before creation.
/// This is a business rule — duplicate names confuse downstream integrations.
/// </summary>
```

### Inline Comments
- Use inline comments **only** for non-obvious logic or business rule explanations.
- Do **not** comment obvious code:
  ```csharp
  // ❌ Do not do this
  // Save the entity to the database
  await _context.SaveChangesAsync();
  ```

### `#region` Blocks in Feature Files
Use `#region` blocks consistently in `<Entity>Features.cs` to organize large CQRS files:

```csharp
#region DTOs
// ...
#endregion

#region Get
// ...
#endregion

#region List
// ...
#endregion

#region Lookup
// ...
#endregion

#region Create
// ...
#endregion

#region Update
// ...
#endregion

#region Delete
// ...
#endregion
```

---

## 5. Async/Await Best Practices

- All I/O-bound operations must be `async/await`. Never use `.Result` or `.Wait()`.
- For multiple independent async operations, use `Task.WhenAll`:
  ```csharp
  // ✅ Run in parallel
  var (projects, users) = await (
      _context.Projects.ToListAsync(),
      _context.Users.ToListAsync()
  );

  // Or explicitly:
  var projectsTask = _context.Projects.ToListAsync();
  var usersTask = _context.Users.ToListAsync();
  await Task.WhenAll(projectsTask, usersTask);
  ```
- Always pass `CancellationToken cancellationToken` through to all EF Core async calls:
  ```csharp
  await _context.SaveChangesAsync(cancellationToken);
  await _context.Customers.FindAsync([id], cancellationToken);
  ```

---

## 6. Dependency Injection Rules

- **Always** inject dependencies via the **constructor**. Never use `IServiceLocator` or `HttpContext.RequestServices` dynamically unless absolutely required (e.g., inside a singleton factory).
- Mark all injected constructor dependencies as `private readonly`:
  ```csharp
  // ✅ Correct
  public class MyHandler
  {
      private readonly IApplicationDbContext _context;
      public MyHandler(IApplicationDbContext context) => _context = context;
  }

  // ✅ Even cleaner — use primary constructors (C# 12, used throughout this project)
  public class MyHandler(IApplicationDbContext context) : IRequestHandler<...>
  {
      // context is implicitly captured
  }
  ```
- Do **not** resolve services inside method bodies:
  ```csharp
  // ❌ Never do this
  var service = HttpContext.RequestServices.GetRequiredService<IMyService>();
  ```

---

## 7. General Code Structure

- Group classes logically in directories matching their domain context (Vertical Slice).
- Use expression body syntax (`=>`) for simple, single-expression members:
  ```csharp
  public string FullName => $"{FirstName} {LastName}";
  ```
- Prefer using `var` for locally-typed variables where the type is obvious from context, but prefer explicit types for clarity in complex assignments.
- Do not use magic numbers or strings — use named constants or enums.
