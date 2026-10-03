# Rule 07: AI Guardrails & Self-Correction

**Scope**: This file is exclusively for AI coding assistants (Antigravity, Copilot, Cursor, etc.).  
**Purpose**: Before submitting any code, run through these mental checkpoints. If any "HALT" condition triggers, stop and course-correct before proceeding.

---

## Pre-Flight Checklist

Before generating or modifying **any** code in this solution, verify:

- [ ] Have I read all the rules in `docs/rules/`?
- [ ] Does any new file I'm creating respect the dependency direction? (`Domain` → `Application` → `Infrastructure` → `Api`)
- [ ] Am I using the correct base class for the handler type (Create/Update/Delete/Get/List)?
- [ ] Does every new Command have a corresponding FluentValidation validator?
- [ ] Am I returning `Result<T>` from every Application handler?
- [ ] Are my string properties initialized to `string.Empty` (not left as uninitialized)?
- [ ] Have I used `#region` blocks in any `XFeatures.cs` file I modified?

---

## HALT Conditions — Stop and Correct Immediately

### ❌ HALT: Business Logic in a Controller

> *"I'm about to write an `if/else` data condition or a `_context.SaveChanges()` inside a Controller action."*

**Correct action:** Move all logic into an Application layer handler. The controller should only contain `return await Mediator.Send(command);`.

---

### ❌ HALT: Returning a Domain Entity to the Client

> *"I'm about to return `Customer` (the domain entity) directly from a handler or controller."*

**Correct action:** Map the entity to a `CustomerDto` (or appropriate DTO record) before returning. Domain entities must never leave the Application layer.

---

### ❌ HALT: Using `DateTime.Now` for Auditing

> *"I'm about to set `entity.CreatedOn = DateTime.Now` or `entity.ModifiedOn = DateTime.UtcNow`."*

**Correct action:** Remove it. The `AuditableEntityInterceptor` in Infrastructure sets all audit timestamps automatically when `SaveChangesAsync` is called. Manual assignment will be overwritten or cause conflicts.

---

### ❌ HALT: Referencing Infrastructure from Application

> *"I added `using Microsoft.EntityFrameworkCore;` or `using Crm.Infrastructure...` inside an Application class."*

**Correct action:** Remove the reference. Application must only depend on Domain. Use `IApplicationDbContext` to access data — never `ApplicationDbContext` directly.

---

### ❌ HALT: Missing FluentValidation Validator

> *"I created a `CreateXCommand` or `UpdateXCommand` but did not create an `AbstractValidator<XCommand>`."*

**Correct action:** Always create paired validators. No command that mutates data should exist without a corresponding validator. The `ValidationBehaviour` can only protect requests if validators are registered.

---

### ❌ HALT: Writing Raw EF Core in a Handler Instead of Base Handler

> *"I'm implementing `IRequestHandler<CreateCustomerCommand, Result<long>>` and writing `_context.Customers.Add(entity); _context.SaveChangesAsync();` from scratch."*

**Correct action:** Inherit from `BaseCreateCommandHandler<Customer, CreateCustomerCommand>` and just override `MapToEntity()`. The base class handles Add + SaveChanges for you.

---

### ❌ HALT: Nullable Warning Being Ignored

> *"A string property I declared doesn't have `?` or `= string.Empty`, but I'm ignoring the nullable warning."*

**Correct action:** Explicitly handle nullability. Add `= string.Empty` for required strings, or add `?` if the property is genuinely optional/nullable.

---

### ❌ HALT: Placing Complex EF Configuration Inside the Domain Entity

> *"I'm about to add `[ForeignKey(...)]`, `[InverseProperty(...)]`, or configure composite keys directly on the entity class."*

**Correct action:** Move complex EF Core configurations to `ApplicationDbContext.OnModelCreating()`. Only use simple `[Required]` and `[MaxLength(N)]` annotations on entity properties.

---

## Quick Reference Decision Tree

```
Starting a new feature?
  └─► Follow 02_entity_creation_workflow.md (4 steps: Domain → Infrastructure → Application → Api)

Modifying existing DB schema?
  └─► Update Domain entity → Update DTO → Update MapToDto + MapToEntity → Run migration

Adding validation?
  └─► AbstractValidator<TCommand> — never in the handler, never in the controller

A query returns nothing?
  └─► throw new NotFoundException(nameof(Entity), id) — never return null from a handler

Returning data to the client?
  └─► Always wrap in Result<T>.Success(data) — never return raw entity or plain DTO from handler

Writing a custom non-CRUD endpoint?
  └─► Use this.OkResponse(...) / this.CreateBadRequest(...) from ApiControllerExtensions

Running parallel async operations?
  └─► Use Task.WhenAll(task1, task2) — never await them sequentially if they are independent
```
