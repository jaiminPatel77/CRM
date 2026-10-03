# Rule 05: Error Handling & Validation

**Scope**: All layers — primarily `Application` (validation) and `Api` (middleware, response models).  
**Enforcement**: Absolute — improper error handling leaks stack traces and breaks client contracts.

---

## 1. `try/catch` — When to Use and When Not To

Avoid `try/catch` in **controllers and Application handlers** for standard flow — the global middleware and `Result<T>` already handle all failure paths cleanly. However, it is **appropriate at the Infrastructure service boundary** when wrapping uncontrolled exceptions into `Result<T>`.

---

### ❌ Do NOT use in: MediatR handlers or standard controllers

The architecture already handles all failure paths. Adding `try/catch` here adds noise and can swallow exceptions that the middleware should see.

```csharp
// ❌ Never do this in a Controller or Handler
public async Task<ActionResult<Result<long>>> Create(CreateCustomerCommand command)
{
    try
    {
        return await Mediator.Send(command);
    }
    catch (Exception ex)
    {
        return BadRequest(ex.Message); // ❌ Bypasses middleware, leaks internals
    }
}

// ✅ Correct — no try/catch needed
public async Task<ActionResult<Result<long>>> Create(CreateCustomerCommand command)
{
    return await Mediator.Send(command); // Pipeline + middleware handles everything
}
```

---

### ✅ DO use in: Infrastructure services to normalize into `Result<T>`

Sources like ASP.NET Identity, external APIs, and third-party SDKs throw raw exceptions. Wrap these **inside the service implementation** to produce clean `Result<T>` objects that controllers can handle without ever seeing a raw exception.

```csharp
// ✅ Inside AuthService (Infrastructure layer) — wrapping Identity/SDK exceptions
public async Task<Result<JwtInfo>> LoginAsync(string email, string password)
{
    try
    {
        var user = await _userManager.FindByEmailAsync(email)
            ?? return Result<JwtInfo>.Failure(["Invalid credentials."]);
        // ... sign in logic
        return Result<JwtInfo>.Success(jwtInfo);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Login failed for {Email}", email);
        return Result<JwtInfo>.Failure(["An unexpected error occurred during login."]);
    }
}
```

### ✅ DO use in: Controllers calling third-party SDKs directly

If a controller (e.g., a webhook handler) must call an external SDK that throws non-standard exceptions and you cannot wrap it in a service:

```csharp
// ✅ OK — 3rd party SDK throws exceptions outside your control
try
{
    await _paymentGateway.ChargeAsync(amount);
    return this.OkResponse(...);
}
catch (PaymentGatewayException ex)
{
    _logger.LogError(ex, "Payment gateway error");
    return this.CreateBadRequest(EnumEntityType.PAYMENT, EnumEntityEvents.PAYMENT_FAILED, ex.Message);
}
```

---

### Decision Summary

| Location | Use `try/catch`? | Reason |
|---|---|---|
| Standard CRUD controller | ❌ No | `Result<T>` + middleware handles it |
| MediatR handler (Application) | ❌ No | Middleware catches unhandled exceptions |
| Auth/custom controller using `Result<T>` service | ❌ No | Service absorbs the error internally |
| Infrastructure service (Identity, email, etc.) | ✅ Yes | Normalize raw exceptions into `Result<T>` |
| Controller calling a 3rd-party SDK directly | ✅ Yes | Catch specific SDK exceptions only |
| Middleware / interceptors | ✅ Yes | They are exception boundaries by design |

---

## 2. Input Validation via FluentValidation

Every command that mutates state (Create, Update) **must** have a FluentValidation `AbstractValidator<TCommand>`.

- The `ValidationBehaviour` MediatR pipeline (`Application/Common/Behaviors/ValidationBehaviour.cs`) automatically executes all validators before the handler runs.
- If validation fails, a `ValidationException` is thrown automatically — the handler never executes.
- **Do not** call validators manually inside handlers.

### Validator Example
```csharp
public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters.");

        RuleFor(v => v.Email)
            .EmailAddress().WithMessage("A valid email address is required.")
            .When(v => v.Email is not null);
    }
}
```

### Rules for Validators
- Validators are **automatically registered** — no manual DI registration needed (handled by `AddApplicationServices()`).
- Update commands must also validate `Id > 0`.
- Use `.When()` for conditional rules.
- Provide descriptive `.WithMessage()` — these messages surface to the client via the API response.

---

## 3. Global Exception Handling Middleware

The `ExceptionHandlingMiddleware` (`src/Crm.Api/Middleware/ExceptionHandlingMiddleware.cs`) is registered in the pipeline and handles all unhandled exceptions.

| Exception Type | HTTP Status | Response Body |
|---|---|---|
| `ValidationException` (FluentValidation) | `400 Bad Request` | `ApiBadRequestResponse` with field-level error list |
| `NotFoundException` | `404 Not Found` | JSON with entity name and id |
| Any unhandled `Exception` | `500 Internal Server Error` | Generic message — **no** stack trace in production |

### Throwing `NotFoundException`
Use `NotFoundException` when a required entity does not exist. It lives in the Application layer and is framework-agnostic.

```csharp
var customer = await _context.Customers.FindAsync([command.Id], cancellationToken)
    ?? throw new NotFoundException(nameof(Customer), command.Id);
```

---

## 4. API Response Shapes

### For Generic Controller Endpoints (inheriting `ApiGenericControllerBase`)

All handlers return `Result<T>` which is passed directly as the HTTP response body:

```json
// Success (200 OK)
{
  "succeeded": true,
  "errors": [],
  "data": { "id": 42, "name": "Alpha Corp" }
}

// Failure — returned for handler-level domain errors
{
  "succeeded": false,
  "errors": ["A customer with this email already exists."],
  "data": null
}
```

### For Custom Controller Endpoints (non-generic, like `AccountController`)

Use the extension methods from `ApiControllerExtensions`:

```csharp
// ✅ Success response
return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.USER_FORGOT_PASSWORD, data);

// ✅ Bad request response
return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_FORGOT_PASSWORD_FAILED, result.Errors);
```

These wrap the response in `ApiOkResponse` / `ApiBadRequestResponse` which include `EntityCode` and `EventCode` metadata for the client.

**Response Shape:**
```json
// ApiOkResponse (200 OK)
{
  "entityCode": "USER",
  "eventCode": "USER_FORGOT_PASSWORD",
  "eventMessageId": "USER_FORGOT_PASSWORD",
  "data": true
}

// ApiBadRequestResponse (400 Bad Request)
{
  "entityCode": "USER",
  "eventCode": "USER_FORGOT_PASSWORD_FAILED",
  "eventMessageId": "USER_FORGOT_PASSWORD_FAILED",
  "errorDetail": "User not found."
}
```

---

## 5. Domain-Level Result Failures vs. Exceptions

Use `Result<T>.Failure(...)` for **expected** business rule rejections (e.g., "email already in use"):

```csharp
// In a handler — expected business failure
if (emailAlreadyExists)
    return Result<long>.Failure(["A customer with this email already exists."]);
```

Use `throw new NotFoundException(...)` for **missing required resources** (records that must exist).

Use the middleware for **unexpected infrastructure errors** — do not catch them yourself.

| Scenario | Mechanism |
|---|---|
| Input validation failure | `AbstractValidator` → `ValidationBehaviour` → `ValidationException` → 400 |
| Resource not found | `throw new NotFoundException(...)` → Middleware → 404 |
| Expected business rule failure | `Result<T>.Failure([...])` → returned in response body |
| Infrastructure/SDK exception | `try/catch` inside the service → `Result<T>.Failure([...])` |
| Unexpected crash in controller/handler | Unhandled exception → Middleware → 500 |

---

## 6. ModelState Validation (Non-Mediator Endpoints Only)

For controllers that **do not** use MediatR (legacy or special-purpose endpoints), check `ModelState.IsValid`:

```csharp
if (!ModelState.IsValid)
    return BadRequest(ModelState);
```

For all Mediator-based controllers, `ModelState` checking is redundant — FluentValidation handles it.
