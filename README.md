# CRM — Multi-Tenant CRM SaaS Foundation

Modern full-stack multi-tenant CRM SaaS foundation built with **Angular 20** and **ASP.NET Core Web API 10** targeting **PostgreSQL 16**.

## Project Identity
- **App Display Name:** CRM
- **C# Namespace / Assembly Prefix:** `Crm`
- **NPM Package Name:** `crm-app`
- **Angular Selector Prefix:** `crm`
- **Database Name:** `crm_db`
- **Storage Key Prefix:** `crm`

---

## Architecture Overview

```
crm-app/
├── backend/
│   ├── src/
│   │   ├── Api/ (Crm.Api)
│   │   ├── Application/ (Crm.Application)
│   │   ├── Domain/ (Crm.Domain)
│   │   ├── Infrastructure/ (Crm.Infrastructure)
│   │   ├── Contracts/ (Crm.Contracts)
│   │   └── Tests/ (Crm.Tests)
│   └── Crm.sln
├── frontend/
│   ├── src/
│   │   ├── app/
│   │   │   ├── core/
│   │   │   ├── features/
│   │   │   │   ├── admin/
│   │   │   │   ├── auth/
│   │   │   │   └── master/
│   │   │   └── shared/
│   │   ├── environments/
│   │   └── styles/
│   ├── angular.json
│   ├── package.json
│   └── vitest.config.ts
└── docker-compose.yml
```

---

## Tech Stack & Features

### Backend (.NET 10 Web API)
- **Clean Architecture & CQRS Pattern** (MediatR, FluentValidation)
- **Entity Framework Core 10** with **PostgreSQL (Npgsql)** provider
- **ASP.NET Core Identity** & **JWT Bearer Authentication**
- **Native ASP.NET Core RateLimiter** (sliding window per IP/tenant)
- **Hangfire** background job processing with PostgreSQL / InMemory fallback
- **Serilog** structured logging
- **Health Checks** endpoint at `/health`

### Frontend (Angular 20 Standalone)
- **Angular 20 Standalone Components & Signals**
- **Bootstrap 5.3 & SCSS Design Tokens** (7 active themes: light, dark, blue, glass, bold, soft, corporate)
- **NgRx Store & Effects** state management
- **Vitest** unit testing setup (46 spec files passing)
- **Optional ReCaptcha** (`enableCaptcha: false` in development/test mode)

---

## Required Configuration

Below is the complete list of setting keys configured in the application. Secret values or environment-specific values should be provided via .NET User Secrets in development or Environment Variables in production.

### Backend Settings Keys (`appsettings.json` / Environment Variables)
| Setting Key Name | Purpose | How to Supply (Dev / Prod) |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection string | `dotnet user-secrets` or `ConnectionStrings__DefaultConnection` |
| `Authentication:JwtIssuerOptions:SecretKey` | HMAC-SHA256 signing key | `dotnet user-secrets` or `Authentication__JwtIssuerOptions__SecretKey` |
| `Authentication:JwtIssuerOptions:Issuer` | Valid JWT token issuer | `appsettings.json` or `Authentication__JwtIssuerOptions__Issuer` |
| `Authentication:JwtIssuerOptions:Audience` | Valid JWT token audience | `appsettings.json` or `Authentication__JwtIssuerOptions__Audience` |
| `DatabaseProvider` | Database engine (`PostgreSQL` / `InMemory`) | `appsettings.json` or `DatabaseProvider` |
| `ReCaptcha:Enabled` | Enable reCAPTCHA verification (`true`/`false`) | `appsettings.json` or `ReCaptcha__Enabled` |
| `ReCaptcha:PrivateKey` | Google reCAPTCHA private secret key | `dotnet user-secrets` or `ReCaptcha__PrivateKey` |
| `ReCaptcha:PublicKey` | Google reCAPTCHA public site key | `appsettings.json` or `ReCaptcha__PublicKey` |
| `Storage:Type` | File storage type (`FileSystem` / `S3` / `Azure`) | `appsettings.json` or `Storage__Type` |
| `Storage:Folder` | File storage path for local file system | `appsettings.json` or `Storage__Folder` |
| `AWS:Profile` | AWS profile name for S3 storage | `appsettings.json` or `AWS__Profile` |
| `AWS:Region` | AWS region name for S3 storage | `appsettings.json` or `AWS__Region` |
| `AWS:BucketName` | AWS S3 bucket name | `appsettings.json` or `AWS__BucketName` |
| `AzureStorage:ConnectionString` | Azure Blob storage connection string | `dotnet user-secrets` or `AzureStorage__ConnectionString` |
| `AzureStorage:ContainerName` | Azure Blob storage container name | `appsettings.json` or `AzureStorage__ContainerName` |
| `EmailSettings:Host` | SMTP server host | `appsettings.json` or `EmailSettings__Host` |
| `EmailSettings:Port` | SMTP server port | `appsettings.json` or `EmailSettings__Port` |
| `EmailSettings:Username` | SMTP authentication username | `dotnet user-secrets` or `EmailSettings__Username` |
| `EmailSettings:Password` | SMTP authentication password | `dotnet user-secrets` or `EmailSettings__Password` |
| `EmailSettings:From` | Default sender email address | `appsettings.json` or `EmailSettings__From` |
| `Pagination:DefaultPageSize` | Default page size for list APIs | `appsettings.json` or `Pagination__DefaultPageSize` |
| `Pagination:MaxPageSize` | Maximum allowed page size for list APIs | `appsettings.json` or `Pagination__MaxPageSize` |
| `ConnectionPool:MinPoolSize` | Minimum connection pool size | `appsettings.json` or `ConnectionPool__MinPoolSize` |
| `ConnectionPool:MaxPoolSize` | Maximum connection pool size | `appsettings.json` or `ConnectionPool__MaxPoolSize` |
| `ConnectionPool:ConnectionTimeout` | Connection timeout in seconds | `appsettings.json` or `ConnectionPool__ConnectionTimeout` |
| `ConnectionPool:ConnectionLifetime` | Connection lifetime in seconds | `appsettings.json` or `ConnectionPool__ConnectionLifetime` |

### Supplying Secrets in Local Development
```bash
cd backend/src/Api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=crm_db;Username=postgres;Password=postgres"
dotnet user-secrets set "Authentication:JwtIssuerOptions:SecretKey" "<your-secret-key-at-least-32-chars>"
```

### Supplying Settings in Environment Variables (Production / Docker)
Use double underscores (`__`) for nested configuration keys in ASP.NET Core:
- `ConnectionStrings__DefaultConnection`
- `Authentication__JwtIssuerOptions__SecretKey`
- `Authentication__JwtIssuerOptions__Issuer`
- `Authentication__JwtIssuerOptions__Audience`

### Frontend Settings Keys (`environment.ts`)
| Setting Key Name | Purpose | Supply Mechanism |
|---|---|---|
| `apiServiceUrl` | Target ASP.NET Core Web API base URL | `environment.ts` / `environment.development.ts` |
| `rootURL` | Application root path | `environment.ts` |
| `enableCaptcha` | Toggle reCAPTCHA verification on login | `environment.ts` |
| `captchaKey` | Public site key for reCAPTCHA | `environment.ts` |
| `remoteLogUrl` | Remote client log collector URL | `environment.ts` |

---

## Getting Started

### 1. Prerequisites
- **Node.js** 20+ LTS and **npm** 10+
- **.NET SDK** 10.0+
- **Docker Desktop** (optional, for running PostgreSQL container)

### 2. Database & Infrastructure
To launch PostgreSQL 16 container locally:
```bash
docker-compose up -d
```

### 3. Backend Setup
```bash
cd backend
dotnet restore Crm.sln
dotnet build Crm.sln
dotnet test Crm.sln
dotnet run --project src/Api/Crm.Api.csproj
```
Health Check endpoint: `http://localhost:5105/health`

### 4. Frontend Setup
```bash
cd frontend
npm install
npx vitest run
npm start
```
Frontend URL: `http://localhost:4200`

---

## Verification & Status (Phase A Complete)
- **Backend Build:** 0 errors
- **Backend Tests:** 42/42 tests passing (`Crm.Tests` & `Crm.Contracts`)
- **Frontend Build:** 0 errors, 0 warnings
- **Frontend Tests:** 46/46 test files passing, 69/69 tests passing (Vitest)
- **Token Sanitization:** 0 hits for legacy company/template tokens
