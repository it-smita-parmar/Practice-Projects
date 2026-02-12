# Coupa Supplier Integration API

Small .NET 10 service for syncing suppliers from Coupa into target systems.

## Projects
- `src/Coupa.Supplier.Api` - HTTP endpoints and middleware.
- `src/Coupa.Supplier.Application` - sync orchestration and validation.
- `src/Coupa.Supplier.Domain` - core contracts/models.
- `src/Coupa.Supplier.Infrastructure` - repository, providers, and external clients.
- `tests/Coupa.Supplier.Infrastructure.Tests` - unit tests (mocked, no DB calls).

## Main endpoints
- `POST /api/suppliers/sync`
- `POST /api/integration/run/{integrationName}`
- `POST /api/integration/outbound/{integrationName}`

## Configuration
Set values in `src/Coupa.Supplier.Api/appsettings.json` (or environment variables):
- `Coupa:BaseUrl`
- `Coupa:SuppliersEndpoint`
- `Coupa:BearerToken`
- `ConnectionStrings:OraclePrimary`

## Run and test
```bash
dotnet build Coupa.Supplier.sln
dotnet test Coupa.Supplier.sln
```
