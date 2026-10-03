# AGENTS.md — CRM SaaS Repository Guidelines

## Project Identity Rules
Always maintain these exact identity values across all C#, TS, SCSS, JSON, and config files:
- **APP_DISPLAY_NAME:** CRM
- **NAMESPACE_PREFIX:** `Crm` (e.g., `Crm.Api`, `Crm.Application`, `Crm.Domain`, `Crm.Infrastructure`)
- **NPM_PACKAGE_NAME:** `crm-app`
- **ANGULAR_SELECTOR_PREFIX:** `crm`
- **DB_NAME:** `crm_db`
- **STORAGE_KEY_PREFIX:** `crm` (e.g., `crm_auth_token`)

---

## Code Quality & Safety Rules
1. **No Abbreviations or Placeholders:** Write complete code. Never write `// rest of code` or partial class placeholders.
2. **Secret Privacy:** NEVER log, print, quote, or commit secret values, API keys, or connection strings. Refer to secrets only by file path and key name.
3. **Verification Before Reporting:** Always run `dotnet build Crm.sln`, `dotnet test Crm.sln`, `npx ng build`, and `npx vitest run` before declaring task completion.
4. **Phased Execution:** Respect phase boundaries and STOP at the end of each phase to wait for explicit user confirmation.

---

## Build & Test Commands

### Backend (.NET 10)
```bash
cd backend
dotnet build Crm.sln
dotnet test Crm.sln
dotnet run --project src/Api/Crm.Api.csproj
```

### Frontend (Angular 20)
```bash
cd frontend
npm install
npx ng build
npx vitest run
```

---

## Key Architectural Principles
- **Backend Architecture:** Clean Architecture with CQRS (MediatR), EF Core 10 (PostgreSQL provider), Identity Core, and native ASP.NET Core RateLimiter.
- **Frontend Architecture:** Angular 20 Standalone components, Signals, SCSS Design System with CSS Custom Properties, NgRx state management, and Vitest test setup.
