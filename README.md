# Coupa Supplier Integration (.NET 10, Clean Architecture)

## Overview
This repository contains a distributed, production-oriented .NET 10 Web API for syncing suppliers from Coupa into Oracle staging tables.

## Solution Structure
- `src/Coupa.Supplier.Api` - presentation/API layer, middleware, health checks, Swagger.
- `src/Coupa.Supplier.Application` - use-cases, DTOs, validation, interfaces.
- `src/Coupa.Supplier.Domain` - aggregate root and value objects.
- `src/Coupa.Supplier.Infrastructure` - Coupa client, Oracle repositories, Unit of Work, strategy selection, token provider.
- `src/Coupa.Supplier.Shared` - shared constants/models.
- `config/coupa-mapping.json` - source-to-target mapping model (field-rename friendly).

## Architecture
- **Clean Architecture / DDD**: domain and application layers do not depend on infrastructure.
- **Strategy Pattern**: `ITargetRepositoryStrategy` chooses `ITargetSupplierRepository` by config (`oracle`/`sql`).
- **Repository + UoW**: `OracleTargetRepository` and `OracleUnitOfWork` isolate persistence concerns.
- **Distributed-ready integration layer**: `ICoupaSupplierClient` isolated from storage and orchestration.
- **Observability**: Application Insights, structured logging, correlation ID middleware.

## Core Flow (`POST /api/suppliers/sync`)
1. Validate request payload.
2. Fetch suppliers from Coupa using bearer token + retry policy (Polly exponential backoff).
3. Validate required fields (`Supplier ID`, `Name`), skip invalid records.
4. Process in configurable batches.
5. Insert using row-by-row or Oracle array binding bulk path.
6. Persist failures into `xxfin.xxfin_coupa_error_logs`.
7. Return summary (`totalReceived`, `totalInserted`, `totalFailed`).

## Target Database Tables
- `xxfin.xxfin_coupa_suppliers`
- `xxfin.xxfin_coupa_supp_sites`
- `xxfin.xxfin_coupa_supp_contacts`
- `xxfin.xxfin_coupa_supp_adds`
- `xxfin.xxfin_coupa_supp_remit_to_adds`
- `xxfin.xxfin_coupa_error_logs` (new)

`STG_ID` from supplier row is used as FK reference in all child tables.

## Configuration
- **Local**: configure token and connection strings in `src/Coupa.Supplier.Api/appsettings.json`.
- **Azure**: set `KeyVault:Url` and use managed identity. Key Vault values override app settings in production.

## Run Locally
> Requires .NET 10 SDK preview and reachable Oracle DB.

```bash
dotnet restore Coupa.Supplier.sln
dotnet build Coupa.Supplier.sln
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/Coupa.Supplier.Api/Coupa.Supplier.Api.csproj
```

Swagger: `http://localhost:<port>/swagger`
Health: `/health`, `/health/live`, `/health/ready`

## Azure App Service Deployment
1. Create App Service (Linux, .NET 10 preview runtime/container).
2. Enable System-Assigned Managed Identity.
3. Grant Key Vault `Get/List` secret access to the App Service identity.
4. Configure App Settings:
   - `KeyVault:Url`
   - `Coupa:BaseUrl`
   - `Coupa:SuppliersEndpoint`
   - `ConnectionStrings:OraclePrimary`
5. Deploy Docker image built from root `Dockerfile` or deploy published artifacts.

## Key Vault Setup
Store secret names such as:
- `Coupa--BearerToken` (maps to `Coupa:BearerToken`)
- `ConnectionStrings--OraclePrimary` (maps to `ConnectionStrings:OraclePrimary`)

## Extensibility Notes
- Field rename only requires `config/coupa-mapping.json` update.
- New target requires new `ITargetSupplierRepository` implementation.
- Bi-directional APIs can be added by introducing reverse use-cases in application layer.
- Multiple Oracle schemas can be selected with additional named repositories/strategies.
