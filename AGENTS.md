# AGENTS.md - Project Rules and Conventions

## GLOBAL RULES
1. **Always write COMPLETE, unabbreviated code**. Never use placeholders like `// rest of the code`, `...`, or `TODO: implement`.
2. **Keep logic simple and direct**. No unnecessary abstractions. No repository pattern on top of EF Core. No MediatR/CQRS. Architectural Flow: `Controller -> Service -> DbContext`.
3. **Do NOT assume versions**. Always use detected stable/LTS versions (.NET 8 LTS, Angular 19, Npgsql EF Core, etc.).
4. **Never commit secrets**. Use `dotnet user-secrets` / environment variables. `appsettings.Development.json` contains safe non-secret defaults only.
5. **Git workflow**: Initialize git at start, make one clean commit at the end of each phase with a descriptive message.
6. **Phase reporting**: After each phase, provide: (a) summary of what was built, (b) exact commands to run it, (c) test results, (d) decisions/assumptions made. Then STOP and wait for `CONFIRM`.
7. **Zero failures policy**: Never report a phase as done while any build error, warning, lint error, or failing test remains.

## TECH STACK
- **Backend**: ASP.NET Core 8 Web API (LTS), C# 12, Entity Framework Core 8, PostgreSQL (Npgsql provider), ASP.NET Core Identity, JWT Bearer authentication, FluentValidation, Serilog structured logging, OpenAPI / Swagger UI.
- **Frontend**: Angular 19 (Standalone Components, Signals, Strict TypeScript, SCSS, Angular Material, Angular Router with lazy-loaded routes, functional guards and functional interceptors).
- **Testing**: Backend xUnit (plain xUnit assertions) + WebApplicationFactory integration tests; Frontend unit tests with Karma/Jasmine + ChromeHeadless; E2E with Playwright.

## PROJECT CONVENTIONS
- Multitenancy: Every tenant-owned entity includes `TenantId` (Guid). `ITenantProvider` extracts `TenantId` from JWT claims. EF Core global query filters enforce tenant isolation across all queries. `TenantId` is NEVER accepted from client request bodies.
- Audit fields: `CreatedAt`, `UpdatedAt` populated automatically in `DbContext.SaveChangesAsync()`.
- Auth token handling: Access tokens stored in-memory only in frontend. Refresh tokens stored in HttpOnly, Secure, SameSite=Strict cookies. Refresh token rotation + reuse detection.
