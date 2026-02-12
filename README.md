# Generic Integration Engine (.NET 10)

## Architecture
This solution now includes a reusable **metadata-driven Integration Engine** aligned with Clean Architecture:

- `src/Coupa.Supplier.Api` → Integration.Host (REST API, middleware, observability)
- `src/Coupa.Supplier.Application` → orchestration/use-case logic
- `src/Coupa.Supplier.Domain` → abstractions + integration contracts
- `src/Coupa.Supplier.Infrastructure` → providers, factories, mapping, persistence
- `src/Coupa.Supplier.Shared` → shared utility models

## Key capabilities delivered
- Bi-directional integration orchestration (`Inbound`, `Outbound`).
- Config-driven source/target/mapping loaded by integration name.
- Dynamic provider selection via factory pattern (no switch-case mapping logic).
- Target providers:
  - Oracle India
  - Oracle US
  - SQL Server
  - MS Dynamics
  - Coupa outbound provider
- Processing modes:
  - Single record mode
  - Bulk mode (batch + configurable parallelism)
- Record-level error capture into SQL Server staging log table (`dbo.IntegrationErrorLog`).
- Mapping engine features:
  - JSONPath-like extraction (`$`, `$.x`, `$.collection[*]`)
  - Nested/parent-child table ordering
  - Parent key propagation
  - Expressions: `constant()`, `now()`, `guid()`, `sequence()`, `upper()`, `concat()`

## Main contracts
### Domain Abstractions
- `IIntegrationOrchestrator`
- `IIntegrationConfigProvider`
- `ISourceProvider`, `ISourceProviderFactory`
- `ITargetProvider`, `ITargetProviderFactory`
- `IMappingEngine`
- `IErrorLogService`
- `ISequenceGenerator`

### Mapping Output
`IMappingEngine.Transform(...)` returns:

```csharp
Dictionary<string, TableBatchData>
```

Where each `TableBatchData` contains:
- `TableName`
- `ColumnData` (`Dictionary<string, List<object?>>`)
- `RowCount`

## Runtime endpoint
Run an integration dynamically by name:

```http
POST /api/integration/run/{integrationName}
POST /api/integration/outbound/{integrationName}
```

Example:

```http
POST /api/integration/run/coupa-supplier-inbound
```

## Config model
A sample full integration config is provided at:

- `config/integrations/coupa-supplier-inbound.json`

It defines:
- `integrationName`
- `direction`
- `source`
- `target`
- `processingMode`
- `batchSize`
- `degreeOfParallelism`
- `mapping.tables[]`

## Observability
- Correlation ID middleware (`X-Correlation-ID`)
- Global exception middleware
- Application Insights telemetry
- Health checks (`/health`, `/health/live`, `/health/ready`)
- Structured logging via ASP.NET + `ILogger`

## Dependency injection
Engine registration is centralized in:

- `AddIntegrationEngine(...)`

This wires configuration provider, mapping engine, all provider strategies, factories, and error logging service.

## Notes on scalability
- Stateless orchestration (no shared mutable in-memory request state)
- Async IO paths end-to-end
- Polly retries on outbound HTTP providers
- Batch + parallel bulk execution
- Factory-driven extensibility for adding new sources/targets without changing orchestration logic


## Endpoint URL configuration
API endpoint paths for Coupa/MS Dynamics are resolved from `appsettings.json` (`IntegrationEngine:ApiEndpoints`) using symbolic values in integration config (e.g., `"endpoint": "@CoupaSuppliersGet"`).
