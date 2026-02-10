# Coupa Invoice Ingestion API (.NET 10)

This project provides a Web API endpoint that:
1. Calls Coupa's **Get Invoices** API.
2. Converts payloads into domain invoice entities.
3. Applies **generic mapping middleware** (profile-driven source→target field mapping).
4. Routes each invoice to one or more targets based on **region/source routing rules**.
5. Persists mapped data to **Oracle staging databases** and supports extension to third-party targets like MS Dynamics.
6. Returns success details in API response once inserts complete.

## Endpoint

- `POST /api/invoices/sync`

Example response:

```json
{
  "totalInvoices": 10,
  "persistedInvoices": 14,
  "message": "Invoices were fetched from Coupa and inserted into configured staging targets.",
  "persistedByTarget": {
    "ORACLE_US": 6,
    "MS_DYNAMICS": 8
  }
}
```

## Design highlights (SOLID + architecture)

- **Single Responsibility**:
  - `CoupaClient` only fetches source data.
  - `InvoiceMappingEngine` only maps profiles.
  - `InvoiceSinkRouter` only resolves targets.
  - `OracleInvoiceSink` only persists to Oracle.
- **Open/Closed**:
  - Add new targets by adding sink type + config entry, no sync flow rewrite.
  - Add mapping profiles via configuration (`InvoiceMappings.Profiles`).
- **Liskov / Interface Segregation**:
  - `ICoupaClient`, `IInvoiceSink`, `IInvoiceSinkRouter`, `IInvoiceMappingEngine` are focused contracts.
- **Dependency Inversion**:
  - Application layer depends on abstractions, infrastructure implements interfaces.

## Generic mapping middleware

`InvoiceMappingMiddleware` executes in the invoice processing pipeline and precomputes mapped payloads per sink/profile. This keeps mapping centralized, reusable, and target-agnostic.

Mapping format:
- Target field => source property path
- Supports direct invoice properties (`InvoiceId`, `Amount`, etc.)
- Supports custom attribute extraction (`attributes.licenseCode`)

## Multi-target regional routing

`TargetRouting.Targets` controls how each invoice routes:
- region filters (`US`, `EU`, `APAC`, ...)
- source filters (`COUPA`, `THIRD_PARTY`, ...)
- sink type (`oracle`, `dynamics`, ...)
- mapping profile (`OracleDefault`, `DynamicsInvoice`, ...)

This enables one source (Coupa) to fan out into multiple Oracle staging DBs and third-party integrations.

## Run

```bash
dotnet restore
dotnet run
```

> Requires .NET SDK with `net10.0` support and valid Oracle connection strings.
