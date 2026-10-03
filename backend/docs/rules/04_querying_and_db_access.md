# Rule 04: Querying & Database Access

**Scope**: `Crm.Application` and `Crm.Infrastructure`.  
**Enforcement**: Absolute — incorrect DB access patterns undermine the architecture.

---

## 1. Access the Database Only Through `IApplicationDbContext`

**Never** inject or reference `ApplicationDbContext` directly in the Application layer. Always use the abstraction interface `IApplicationDbContext`.

```csharp
// ✅ Correct
public class GetCustomerQueryHandler(IApplicationDbContext context)
    : BaseGetQueryHandler<Customer, CustomerDto, GetCustomerQuery>(context)

// ❌ Wrong — Application should not depend on Infrastructure
public class GetCustomerQueryHandler(ApplicationDbContext context) ...
```

The interface `IApplicationDbContext` is defined in `Application/Common/Interfaces/IApplicationDbContext.cs` and implemented in `Infrastructure/Persistence/ApplicationDbContext.cs`.

---

## 2. Use Generic Base Handlers Instead of Raw `IRequestHandler`

The Application layer contains powerful base handlers that abstract away all repetitive CRUD logic. **Always** inherit from these instead of writing raw handlers unless the logic is fundamentally unique.

| Base Handler | Purpose |
|---|---|
| `BaseGetQueryHandler<TEntity, TDto, TQuery>` | Fetches a single record by `Id`, maps to DTO |
| `BaseListQueryHandler<TEntity, TDto, TQuery>` | Paginates, filters, and sorts using Gridify |
| `BaseCreateCommandHandler<TEntity, TCommand>` | Adds entity to DbContext and saves |
| `BaseUpdateCommandHandler<TEntity, TCommand>` | Finds entity, maps updates, saves |
| `BaseDeleteCommandHandler<TEntity, TCommand>` | Finds entity, removes it, saves |

**Files:** `src/Crm.Application/Common/Features/GenericBaseFeatures.cs`

---

## 3. Pagination, Filtering & Sorting with Gridify

All list endpoints use the **Gridify** library for server-side, safe, dynamic filtering and sorting. This translates HTTP query string parameters directly into EF Core `IQueryable` SQL — **without** raw SQL.

### Query String Format

| Parameter | Purpose | Example |
|---|---|---|
| `page` | Page number (1-based) | `?page=2` |
| `pageSize` | Records per page | `?pageSize=25` |
| `filter` | Dynamic field filters | `?filter=name=*Alpha*,status=1` |
| `orderBy` | Sort expression | `?orderBy=createdOn desc` |

**Example URL:** `GET /api/v1/Projects?page=1&pageSize=10&filter=name=*Al*&orderBy=id desc`

### How it Works in the Handler

Your List Query record inherits from `GridifyQuery`:
```csharp
public record GetCustomersWithPaginationQuery : GridifyQuery, IRequest<Result<Paging<CustomerDto>>> { }
```

The base `BaseListQueryHandler` automatically calls:
```csharp
var result = await queryable.GridifyAsync(request, cancellationToken);
```

### Custom Overrides

If you need to include related entities (`.Include()`), override `GetQueryable()`:
```csharp
protected override IQueryable<Customer> GetQueryable()
    => _context.Customers
        .Include(c => c.Orders)
        .AsNoTracking();
```

> **Note:** Always add `.AsNoTracking()` on read-only list/lookup queries for better performance. EF Core does not need to track entities that are only being read.

---

## 4. Lookup Queries (Dropdowns)

Lookup queries return slim DTOs (just `Id` + display `Name`) for use in dropdown menus. They are implemented as custom `IRequestHandler` classes since they differ significantly from list queries.

```csharp
public class GetCustomersLookupQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCustomersLookupQuery, Result<Paging<CustomerLookupDto>>>
{
    public async Task<Result<Paging<CustomerLookupDto>>> Handle(
        GetCustomersLookupQuery request, CancellationToken cancellationToken)
    {
        var paging = await context.Customers
            .AsNoTracking()
            .Select(c => new CustomerLookupDto { Id = c.Id, Name = c.Name })
            .GridifyAsync(request);

        return Result<Paging<CustomerLookupDto>>.Success(paging);
    }
}
```

> **Key rules for lookup handlers:**
> - Always use `.AsNoTracking()`.
> - Always use `.Select()` to project only needed fields (avoid fetching the whole entity).
> - Return `Result<Paging<CustomerLookupDto>>` — do not return raw lists.

---

## 5. Complex Queries (Joins, Aggregates)

For queries that go beyond simple `DbSet<T>` access:

- Write the query using LINQ to Entities (strongly typed). Prefer method syntax over query syntax for readability.
- Use `.Select()` projections to avoid over-fetching data.
- Use `.AsSplitQuery()` for queries with multiple `.Include()` collections (prevents cartesian explosion):
  ```csharp
  var data = await _context.Projects
      .Include(p => p.Tasks)
      .Include(p => p.Members)
      .AsSplitQuery()
      .ToListAsync(cancellationToken);
  ```
- Prefer `FirstOrDefaultAsync` over `FirstAsync` when the item might not exist, and handle the `null` case explicitly by throwing `NotFoundException`.

---

## 6. Saving Data

- Always use `await _context.SaveChangesAsync(cancellationToken)` — never the synchronous `.SaveChanges()`.
- The `AuditableEntityInterceptor` automatically sets `CreatedOn`, `CreatedById`, `ModifiedOn`, and `ModifiedById` before saving — **do not set these manually**.
- After saving, return the entity's `Id` using `Result<long>.Success(entity.Id)`.

---

## 7. NotFoundException Pattern

When a required entity is not found, throw `NotFoundException` from the Application layer:

```csharp
var entity = await _context.Customers.FindAsync([id], cancellationToken)
    ?? throw new NotFoundException(nameof(Customer), id);
```

The global `ExceptionHandlingMiddleware` will automatically translate this into an HTTP `404 Not Found` response with a structured JSON body. **Never** return `null` or check `entity == null` in the handler body and return a failure result manually for not-found cases.
