# Multi-Tenant CRM SaaS Solution

A multi-tenant CRM SaaS built with ASP.NET Core Web API (.NET 8 LTS) and Angular 19 (Standalone Components, Signals, Angular Material).

## Architecture & Structure
```text
crm/
  backend/
    src/Crm.Api/                (ASP.NET Core Web API, controllers, EF Core DbContext)
    tests/Crm.Api.UnitTests/    (xUnit unit tests)
    tests/Crm.Api.IntegrationTests/ (WebApplicationFactory & Testcontainers integration tests)
  frontend/                     (Angular 19 application)
  docker-compose.yml            (PostgreSQL for local development)
  README.md                     (Setup & instructions)
  AGENTS.md                     (Project rules and tech stack standards)
```

## Prerequisites
- **.NET 8.0 SDK** (or .NET 10 SDK targeting `net8.0`)
- **Node.js** v24+ & **npm** 11+
- **Docker Desktop** (for PostgreSQL database)

## Quick Start (Local Setup)

### 1. Database Setup
Start the local PostgreSQL container via Docker Compose:
```bash
docker compose up -d
```

### 2. Backend API Setup
Run EF Core database migrations and start the ASP.NET Core API server:
```bash
# Apply EF Core Migrations
dotnet ef database update --project backend/src/Crm.Api/Crm.Api.csproj

# Run Backend API
dotnet run --project backend/src/Crm.Api/Crm.Api.csproj
```
The API will start at `https://localhost:5001` / `http://localhost:5000` with Swagger UI at `http://localhost:5000/swagger` and healthcheck at `http://localhost:5000/health`.

### 3. Frontend Setup
Install Angular dependencies and start dev server:
```bash
cd frontend
npm install
npm start
```
The Angular frontend will run at `http://localhost:4200` with requests to `/api` automatically proxied to the backend (`http://localhost:5000`).

## Running Tests

### Backend Unit & Integration Tests
```bash
dotnet test backend/Crm.slnx
```

### Frontend Unit Tests
```bash
cd frontend
npm test
```

### Frontend Build Verification
```bash
cd frontend
npm run build
```
