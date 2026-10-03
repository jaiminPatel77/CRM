# Rule 02: Entity Creation Workflow (End-to-End CRUD)

**Scope**: Creating any new database-backed entity with full API exposure.  
**When to apply**: Whenever the user asks to "add a new entity", "create a new model", or "add CRUD for X".

---

## Overview

Every new entity follows a strict **4-step workflow**. Do not skip or reorder steps.

```
Step 1: Domain Entity
Step 2: EF Core Registration + Migration
Step 3: Application Features (CQRS)
Step 4: API Controller
```

---

## Step 1: Domain Entity

**Location:** `src/Crm.Domain/Entities/`  
**File:** `<EntityName>.cs`

### Rules
- Inherit from `AuditableEntity` (not `BaseEntity` directly, unless the entity does not need audit fields).
- Use file-scoped namespace: `namespace Crm.Domain.Entities;`
- Use `[Required]` and `[MaxLength(N)]` for validation constraints.
- **Do not** use `[Table("Name")]`.
- Initialize non-nullable strings: `= string.Empty;`
- If the entity has child collections:
  ```csharp
  public virtual IList<ChildEntity> Children { get; private set; } = [];
  ```

### Example
```csharp
using Crm.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Crm.Domain.Entities;

public class Customer : AuditableEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Email { get; set; }

    public virtual IList<Order> Orders { get; private set; } = [];
}
```

---

## Step 2: EF Core Registration + Migration

**Location:** `src/Crm.Infrastructure/Persistence/ApplicationDbContext.cs`

### Actions
1. Add `DbSet<T>` to `ApplicationDbContext`:
   ```csharp
   public DbSet<Customer> Customers { get; set; } = null!;
   ```
2. If complex constraints are needed (composite keys, cascade rules), add them to `OnModelCreating()` — **not** on the entity class.
3. **Inform the user** to run EF Migrations via CLI:
   ```shell
   dotnet ef migrations add Add<EntityName>Entity \
     --project src\Crm.Infrastructure \
     --startup-project src\Crm.Api
   
   dotnet ef database update \
     --project src\Crm.Infrastructure \
     --startup-project src\Crm.Api
   ```

> **Note:** Never run migrations automatically. Always surface the CLI command to the developer.

---

## Step 3: Application Features (CQRS Vertical Slice)

**Location:** `src/Crm.Application/Features/<EntityName>/`  
**File:** `<EntityName>Features.cs`

> **Pro Tip:** Copy `ProjectFeatures.cs` from `Features/Projects/` and do a rename-all `Project` → `<EntityName>`. Then adjust property mappings.

Use `#region` blocks to organize the file:

```
#region DTOs
#region Get (Single)
#region List (Paginated)
#region Lookup
#region Create
#region Update
#region Delete
```

### 3a. DTOs

```csharp
public record CustomerDto : BaseAuditableDto<long>
{
    public string Name { get; init; } = string.Empty;
    public string? Email { get; init; }
}

public record CustomerLookupDto
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
```

### 3b. Get (Single Record) Query

```csharp
public record GetCustomerQuery : IRequest<Result<CustomerDto>>, IHasId<long>
{
    public long Id { get; set; }
}

public class GetCustomerQueryHandler(IApplicationDbContext context)
    : BaseGetQueryHandler<Customer, CustomerDto, GetCustomerQuery>(context)
{
    protected override CustomerDto MapToDto(Customer entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Email = entity.Email,
        // ... audit fields
    };

    // Only override GetQueryable if you need .Include():
    // protected override IQueryable<Customer> GetQueryable()
    //     => _context.Customers.Include(c => c.Orders);
}
```

### 3c. List (Paginated) Query

```csharp
public record GetCustomersWithPaginationQuery : GridifyQuery, IRequest<Result<Paging<CustomerDto>>> { }

public class GetCustomersWithPaginationQueryHandler(IApplicationDbContext context)
    : BaseListQueryHandler<Customer, CustomerDto, GetCustomersWithPaginationQuery>(context)
{
    protected override CustomerDto MapToDto(Customer entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        // ...
    };
}
```

### 3d. Lookup Query (for dropdowns)

The lookup query is a custom handler returning a slimmer DTO set for dropdown consumption.

```csharp
public record GetCustomersLookupQuery : GridifyQuery, IRequest<Result<Paging<CustomerLookupDto>>> { }

public class GetCustomersLookupQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCustomersLookupQuery, Result<Paging<CustomerLookupDto>>>
{
    public async Task<Result<Paging<CustomerLookupDto>>> Handle(
        GetCustomersLookupQuery request, CancellationToken cancellationToken)
    {
        var paging = await context.Customers
            .Select(c => new CustomerLookupDto { Id = c.Id, Name = c.Name })
            .GridifyAsync(request);

        return Result<Paging<CustomerLookupDto>>.Success(paging);
    }
}
```

### 3e. Create Command

```csharp
public record CreateCustomerCommand : IRequest<Result<long>>
{
    public string Name { get; init; } = string.Empty;
    public string? Email { get; init; }
}

/// <summary>Validates inputs before the handler executes via MediatR pipeline.</summary>
public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(v => v.Name).NotEmpty().MaximumLength(200);
        RuleFor(v => v.Email).EmailAddress().When(v => v.Email is not null);
    }
}

public class CreateCustomerCommandHandler(IApplicationDbContext context)
    : BaseCreateCommandHandler<Customer, CreateCustomerCommand>(context)
{
    protected override Customer MapToEntity(CreateCustomerCommand command) => new()
    {
        Name = command.Name,
        Email = command.Email,
    };
}
```

### 3f. Update Command

```csharp
public record UpdateCustomerCommand : IRequest<Result<long>>, IHasId<long>
{
    public long Id { get; set; }
    public string Name { get; init; } = string.Empty;
    public string? Email { get; init; }
}

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(v => v.Id).GreaterThan(0);
        RuleFor(v => v.Name).NotEmpty().MaximumLength(200);
    }
}

public class UpdateCustomerCommandHandler(IApplicationDbContext context)
    : BaseUpdateCommandHandler<Customer, UpdateCustomerCommand>(context)
{
    protected override void MapToEntity(UpdateCustomerCommand command, Customer entity)
    {
        entity.Name = command.Name;
        entity.Email = command.Email;
    }
}
```

### 3g. Delete Command

```csharp
public record DeleteCustomerCommand : IRequest<Result<long>>, IHasId<long>, new()
{
    public long Id { get; set; }
}

public class DeleteCustomerCommandHandler(IApplicationDbContext context)
    : BaseDeleteCommandHandler<Customer, DeleteCustomerCommand>(context)
{ }
```

---

## Step 4: API Controller

**Location:** `src/Crm.Api/Controllers/`  
**File:** `CustomersController.cs`

```csharp
using Crm.Application.Common.Models;
using Crm.Application.Features.Customers;
using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers;

[Authorize]
[ApiController] // Required for strict Architecture Test
public class CustomersController : ApiGenericControllerBase<
    CreateCustomerCommand,
    UpdateCustomerCommand,
    DeleteCustomerCommand,
    GetCustomerQuery,
    GetCustomersWithPaginationQuery,
    CustomerDto>
{
    // The base class auto-provides:
    //   GET    /api/v1/Customers/{id}
    //   GET    /api/v1/Customers
    //   POST   /api/v1/Customers
    //   PUT    /api/v1/Customers/{id}
    //   DELETE /api/v1/Customers/{id}

    // Lookup endpoint for dropdowns
    [HttpGet("lookup-list")]
    [ProducesResponseType(typeof(Result<Paging<CustomerLookupDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<Paging<CustomerLookupDto>>>> GetLookupList(
        [FromQuery] GetCustomersLookupQuery query)
    {
        return await Mediator.Send(query);
    }
}
```

### ✅ Done! You now have:
- Validated, paginated, fully RESTful CRUD endpoints at `/api/v1/Customers`
- Swagger/Scalar documentation auto-generated
- Auditing fields auto-stamped on create/update

---

## Checklist Before Committing

- [ ] Domain entity inherits `AuditableEntity`
- [ ] `DbSet<T>` added to `ApplicationDbContext`
- [ ] EF Migration created and applied
- [ ] `XFeatures.cs` created with all 7 sections (DTOs, Get, List, Lookup, Create, Update, Delete)
- [ ] FluentValidation validators exist for Create and Update commands
- [ ] Controller inherits `ApiGenericControllerBase` with correct generic types
- [ ] `[Authorize]` and `[ApiController]` attributes are present on the controller
- [ ] No business logic inside the controller
