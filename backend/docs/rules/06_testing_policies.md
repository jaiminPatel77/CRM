# Rule 06: Testing Policies

**Scope**: `Crm.Tests` project.  
**When to apply**: Whenever writing or modifying unit or integration tests.

---

## 1. Test Project Structure

Mirror the directory structure of the `Application` layer. Each handler or feature should have its own test file.

```
Crm.Tests/
  └── Features/
        └── Customers/
              └── CreateCustomerCommandTests.cs
              └── GetCustomerQueryTests.cs
              └── UpdateCustomerCommandTests.cs
  └── Common/
        └── ValidationBehaviourTests.cs
```

The path pattern for a test file: `Tests/Features/<EntityName>/<CommandOrQuery>Tests.cs`

---

## 2. Test Method Naming Convention

Use the **`MethodName_StateUnderTest_ExpectedBehavior`** pattern:

```csharp
// ✅ Correct
[Fact]
public async Task Handle_ValidCommand_ReturnsSuccessWithId() { ... }

[Fact]
public async Task Handle_DuplicateEmail_ReturnsFailure() { ... }

[Fact]
public async Task Handle_EntityNotFound_ThrowsNotFoundException() { ... }

// ❌ Wrong — vague names
public async Task Test1() { ... }
public async Task CreatesCustomer() { ... }
```

---

## 3. Assertion Library: FluentAssertions

Always use **FluentAssertions** for all assertions. Never use `Assert.Equal(...)` style from xUnit directly.

```csharp
// ✅ Correct
result.Should().NotBeNull();
result.Succeeded.Should().BeTrue();
result.Data.Should().Be(1);
result.Errors.Should().BeEmpty();

// ❌ Wrong
Assert.NotNull(result);
Assert.True(result.Succeeded);
```

---

## 4. Mocking: Moq for `IApplicationDbContext`

**Unit Tests** for Application handlers must **not** hit a real database. Use `Moq` to mock `IApplicationDbContext`.

```csharp
public class CreateCustomerCommandTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly CreateCustomerCommandHandler _handler;

    public CreateCustomerCommandTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();

        // Set up in-memory DbSet behavior
        var customers = new List<Customer>();
        var mockDbSet = customers.AsQueryable().BuildMockDbSet(); // Use MockQueryable.Moq
        _contextMock.Setup(x => x.Customers).Returns(mockDbSet.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(1);

        _handler = new CreateCustomerCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithId()
    {
        // Arrange
        var command = new CreateCustomerCommand { Name = "Alpha Corp" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

---

## 5. Test Categories

### Unit Tests (Application Layer)
- Mock `IApplicationDbContext` with `Moq`.
- Test each handler in isolation.
- Test FluentValidation validators independently too.

### Validator Tests

```csharp
public class CreateCustomerCommandValidatorTests
{
    private readonly CreateCustomerCommandValidator _validator = new();

    [Fact]
    public void Validate_EmptyName_FailsValidation()
    {
        var command = new CreateCustomerCommand { Name = string.Empty };
        var result = _validator.TestValidate(command); // FluentValidation.TestHelper
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var command = new CreateCustomerCommand { Name = "Alpha Corp" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
```

---

## 6. Required NuGet Packages for Tests

| Package | Purpose |
|---|---|
| `xUnit` | Test runner |
| `FluentAssertions` | Assertion library |
| `Moq` | Mocking framework |
| `MockQueryable.Moq` | Mock `IQueryable<T>` / `DbSet<T>` for EF Core |
| `FluentValidation.TestHelper` | Test validators using `.TestValidate()` and `.ShouldHaveValidationErrorFor()` |

---

## 7. Test Isolation Rules

- Each test method must be independent — do not share mutable state between tests.
- Use `[Fact]` for single-scenario tests.
- Use `[Theory]` + `[InlineData(...)]` for parameterized / multiple-scenario tests:
  ```csharp
  [Theory]
  [InlineData("")]
  [InlineData(null)]
  [InlineData("   ")]
  public void Validate_InvalidName_FailsValidation(string? name)
  {
      var command = new CreateCustomerCommand { Name = name! };
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(c => c.Name);
  }
  ```
