# AI Workspace Rules — Index

**For AI coding assistants:** Read the files relevant to your current task before making any changes.

| # | File | Category | When to Use |
|---|---|---|---|
| 01 | [01_architecture.md](01_architecture.md) | Core Architecture | Before touching any layer; understanding dependency rules, CQRS, response shapes |
| 02 | [02_entity_creation_workflow.md](02_entity_creation_workflow.md) | Entity Creation | Creating any new entity with CRUD APIs (step-by-step with code examples) |
| 03 | [03_coding_standards.md](03_coding_standards.md) | Coding Standards | Writing any C# code; naming, record types, string handling, async, DI |
| 04 | [04_querying_and_db_access.md](04_querying_and_db_access.md) | DB Access & Querying | Accessing the database, Gridify pagination, lookup queries, saving data |
| 05 | [05_error_handling_and_validation.md](05_error_handling_and_validation.md) | Error Handling | FluentValidation validators, exception types, response shapes |
| 06 | [06_testing_policies.md](06_testing_policies.md) | Testing | Writing unit tests with Moq, FluentAssertions, and validator testing |
| 07 | [07_ai_guardrails.md](07_ai_guardrails.md) | AI Guardrails | **Always read** — pre-flight checklist and HALT conditions for AI assistants |

---

## Quick Start

- **Adding a new entity?** → Read [02](02_entity_creation_workflow.md), follow the 4-step workflow.
- **Changing DB schema?** → Read [01](01_architecture.md) + [04](04_querying_and_db_access.md).
- **Writing any C# code?** → Read [03](03_coding_standards.md).
- **Handling errors?** → Read [05](05_error_handling_and_validation.md).
- **Writing tests?** → Read [06](06_testing_policies.md).
- **Before submitting any code?** → Run through [07](07_ai_guardrails.md).

## 🧪 Testing & Code Coverage

The project includes a comprehensive test suite covering Domain, Application, and API layers.

### Run Tests
To execute all tests and collect coverage data:
```powershell
dotnet test --collect:"XPlat Code Coverage" /p:Threshold=0
```

### Generate HTML Coverage Report
We use **ReportGenerator** to visualize code coverage. 

1. **Install ReportGenerator** (if not already installed):
   ```powershell
   dotnet tool install -g dotnet-reportgenerator-globaltool
   ```

2. **Generate the Report**:
   From the project root, run:
   ```powershell
   reportgenerator "-reports:src\Crm.Tests\TestResults\*\coverage.cobertura.xml" "-targetdir:src\Crm.Tests\TestResults\CoverageReport" -reporttypes:Html
   ```

3. **View the Report**:
   Open `src\Crm.Tests\TestResults\CoverageReport\index.html` in your browser.

---

## 📖 Documentation
- [Developer Guide](docs/developer_guide): Deep dive into architecture, patterns, and coding standards.
- [Architecture Rules](docs/rules/README.md): Automated architectural enforcement rules.
