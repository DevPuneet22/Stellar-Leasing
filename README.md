# Stellar Leasing

Stellar Leasing is a Qntrl-style workflow platform in active development. The current build covers the workflow-definition slice end to end: PostgreSQL persistence, versioned workflow definitions, a drag-and-drop builder, tenant-scoped reads and writes, optimistic concurrency protection, and local demo authentication.

## Current Product Slice

Implemented now:

- workflow definition CRUD with draft and active versions
- workflow activation and next-version creation
- drag-and-drop workflow builder with node positioning
- backend validation for invalid graphs and stale edits
- PostgreSQL-backed storage through EF Core and Npgsql
- background worker startup and heartbeat
- local JWT login for the workflow area

Not implemented yet:

- provider-backed SSO and production RBAC
- workflow runtime execution and task inbox
- comments, attachments, notifications, and SLA automation
- reporting, integrations, and admin settings

## Tech Stack

- backend: `ASP.NET Core 10`, `EF Core`, `PostgreSQL`
- frontend: `React`, `TypeScript`, `Vite`, `React Flow`
- local infrastructure: `Docker Compose`

## Repository Layout

```text
backend/
  src/
    StellarLeasing.Domain/
    StellarLeasing.Application/
    StellarLeasing.Infrastructure/
    StellarLeasing.Api/
    StellarLeasing.Worker/
frontend/
docker/
docs/
```

## Prerequisites

- `.NET SDK 10`
- `Node.js` and `npm`
- `Docker Desktop`

## Local Setup

Run everything from:

```powershell
C:\Users\PuneetGupta\Desktop\Stellar-Leasing
```

### 1. Start PostgreSQL

```powershell
docker compose -f docker/docker-compose.yml up -d
```

The project uses host port `55432`.

If your local database was created earlier with different credentials, reset it with:

```powershell
docker compose -f docker/docker-compose.yml down -v
docker compose -f docker/docker-compose.yml up -d
```

### 2. Start the API

```powershell
dotnet run --project backend/src/StellarLeasing.Api/StellarLeasing.Api.csproj --launch-profile http
```

### 3. Start the worker

```powershell
dotnet run --project backend/src/StellarLeasing.Worker/StellarLeasing.Worker.csproj
```

### 4. Start the frontend

Install packages once:

```powershell
cd frontend
npm install
```

Then run:

```powershell
npm run dev
```

## Login

The local environment uses a demo administrator account:

- email: `admin@stellar.local`
- password: `ChangeMe123!`

Sign in at:

- `http://127.0.0.1:5173/login`

## Local URLs

- frontend: `http://127.0.0.1:5173`
- workflow catalog: `http://127.0.0.1:5173/workflows`
- API health: `http://localhost:5080/api/system/health`
- workflow definitions API: `http://localhost:5080/api/workflow-definitions`

## Environment Defaults

The default local values are defined in:

- `.env.example`
- `backend/src/StellarLeasing.Api/appsettings.Development.json`
- `backend/src/StellarLeasing.Worker/appsettings.Development.json`

Important defaults:

- Postgres port: `55432`
- database: `stellarleasing`
- database user: `stellar`
- database password: `stellar_dev_password`
- default tenant: `11111111-1111-1111-1111-111111111111`

## Build Checks

Backend:

```powershell
dotnet build backend/StellarLeasing.sln
```

Frontend:

```powershell
cd frontend
npm run build
```

## What To Test

- sign in with the demo account
- create a workflow
- drag nodes onto the builder canvas
- connect steps with transitions
- save the draft
- activate a version
- create the next draft version
- confirm stale edits are rejected if the workflow changed

## Next Product Slice

The highest-value remaining work is:

1. provider-backed authentication and role-based access control
2. runtime workflow execution
3. task inbox and approval actions
4. audit trail, comments, attachments, notifications, and SLA jobs

## Notes

- the workflow builder and catalog are protected by local JWT auth
- tenant resolution now comes from the authenticated user claim first
- the current auth implementation is suitable for local development, not production
