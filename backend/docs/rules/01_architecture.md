# Rule 01: Core Architecture

**Scope**: All layers of the `Crm` solution.  
**Enforcement**: Absolute — violations are critical failures.

---

## 1. Dependency Inversion (The Golden Rule)

The project follows strict **Clean Architecture**. Inner layers must **never** depend on outer layers.

| Layer | Allowed Dependencies |
|---|---|
| `Domain` | **None**. Zero references to any other project. |
| `Application` | `Domain` only. |
| `Infrastructure` | `Application` + `Domain`. |
| `Api` | `Infrastructure` + `Application`. |

**❌ Critical Failures — Always refuse and correct:**
- A `using` statement referencing `Microsoft.EntityFrameworkCore` inside any `Domain` class.
- A `using` referencing `Infrastructure` from within `Application`.
- A direct `DbContext` call inside an `Application` handler without going through `IApplicationDbContext`.

**How DI is wired at startup** (`src/Crm.Api/Program.cs`):
```csharp
builder.Services.AddApplicationServices();       // Registers MediatR, FluentValidation, Behaviors
builder.Services.AddInfrastructureServices(...); // Registers EF Core, Identity, Hangfire
```

---

## 2. CQRS Strictness

This project implements **CQRS** (Command Query Responsibility Segregation) via **MediatR**. Commands mutate state; Queries read state.

### Controllers
- Controllers **only** route HTTP requests to MediatR. Their body should look like:
  ```csharp
  return await Mediator.Send(command);
  ```
- **Never** put business logic, conditional branching on data, or data-access code inside a controller.
- Use `ApiGenericControllerBase` (`src/Crm.Api/Controllers/ApiGenericControllerBase.cs`) for all standard CRUD entities — you inherit 5 endpoints automatically.

### Application Layer
- All business logic lives in `Crm.Application/Features/<EntityName>/`.
- Each feature file (`XFeatures.cs`) uses **Vertical Slicing** — one file holds all DTOs, Queries, Commands, Validators, and Handlers for that entity. Do not break these across separate files.
- Handlers must access data **only** through `IApplicationDbContext` (injected via constructor DI).

### Validators
- **Every** command that mutates data must have an associated `AbstractValidator<TCommand>` (FluentValidation).
- The `ValidationBehaviour` MediatR pipeline automatically runs validators before the handler executes. No manual validation calls are needed inside handlers.

---

## 3. Project Layer Summary

```
Crm.Domain
  └── Entities (POCO classes : AuditableEntity)
  └── Common (BaseEntity, AuditableEntity, Enums, Domain Events)

Crm.Application
  └── Features/<Entity>/
        └── <Entity>Features.cs  (DTOs, Commands, Queries, Validators, Handlers)
  └── Common/
        └── Behaviors/           (ValidationBehaviour, etc.)
        └── Features/            (Generic base handlers)
        └── Interfaces/          (IApplicationDbContext, etc.)
        └── Models/              (Result<T>, Paging<T>)

Crm.Infrastructure
  └── Persistence/
        └── ApplicationDbContext.cs
        └── Migrations/
  └── Services/                  (Email, Auth, etc.)

Crm.Api
  └── Controllers/               (ApiControllerBase, ApiGenericControllerBase, feature controllers)
  └── Middleware/                (ExceptionHandlingMiddleware)
  └── Models/                    (ApiResponse, ApiOkResponse, ApiBadRequestResponse)
```

---

## 4. EF Core Conventions

- **Do not** use `[Table("Name")]` data annotations. Let EF Core infer table names from `DbSet<T>` property names on `ApplicationDbContext`.
- Use `[Required]` and `[MaxLength(N)]` data annotations on entity properties.
- Complex constraints (composite keys, `OnDelete(Cascade)`) belong in `ApplicationDbContext.OnModelCreating()`, not on the entity class.
- **Always** mark EF Core navigational properties as `virtual`:
  ```csharp
  public virtual IList<Task> Tasks { get; private set; } = [];
  ```
  This enables EF Core lazy-loading / proxy generation if configured.
- Auditing (`CreatedOn`, `CreatedById`, `ModifiedOn`, `ModifiedById`) is handled **automatically** by `AuditableEntityInterceptor`. Never set these manually or stamp them inside `OnModelCreating`.

---

## 5. Response Shape Contracts

All Application handlers return `Result<T>` from `Crm.Application/Common/Models/Result.cs`:

```csharp
// Success
return Result<long>.Success(entity.Id);

// Failure
return Result<long>.Failure(["Entity not found."]);
```

The response has three fields: `Succeeded`, `Errors[]`, and `Data`. Controllers pass this directly to the HTTP response — no additional wrapping needed for generic controller endpoints.

For non-generic controllers (e.g., `AccountController`), use the extension helpers from `ApiControllerExtensions`:
```csharp
return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.COMMON_GET_ITEM, data);
return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.COMMON_PUT_EXCEPTION, errors);
```
