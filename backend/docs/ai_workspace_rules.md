# AI Workspace Rules: Crm

> **⚠️ This file is a high-level summary. The detailed, authoritative rules have been split into categorized files.**
> **AI Assistants: Read the relevant files in [`docs/rules/`](rules/README.md) before making any changes.**

---

## Rule Files Index

| File | Category |
|---|---|
| [01_architecture.md](rules/01_architecture.md) | Core Architecture, Dependency Inversion, CQRS, EF Conventions |
| [02_entity_creation_workflow.md](rules/02_entity_creation_workflow.md) | End-to-end workflow for new entities with full CRUD |
| [03_coding_standards.md](rules/03_coding_standards.md) | .NET 10 C# conventions, naming, async, DI, documentation |
| [04_querying_and_db_access.md](rules/04_querying_and_db_access.md) | IApplicationDbContext, Gridify pagination, lookup patterns |
| [05_error_handling_and_validation.md](rules/05_error_handling_and_validation.md) | FluentValidation, exception middleware, response shapes |
| [06_testing_policies.md](rules/06_testing_policies.md) | Test structure, Moq, FluentAssertions, validator testing |
| [07_ai_guardrails.md](rules/07_ai_guardrails.md) | ❗ Pre-flight checklist and HALT conditions for AI assistants |

---

## Quick Rules Summary

1. **Layer isolation is absolute** — `Domain` has no deps. `Application` depends only on `Domain`. Never cross these boundaries.
2. **Controllers only route** — they contain `return await Mediator.Send(command);` and nothing else.
3. **All business logic is in the Application layer handlers.**
4. **Every mutating command must have a FluentValidation validator.**
5. **Never return a Domain Entity to the client** — always map to a DTO.
6. **Use `Result<T>` from every handler** for standardized response shapes.
7. **Never `try/catch` in controllers or handlers** — the global `ExceptionHandlingMiddleware` handles everything.
8. **Use file-scoped namespaces, records for DTOs/Commands, `init` properties, and `string.Empty` for non-nullable strings.**

---

*Full rules: [docs/rules/](rules/README.md)*
