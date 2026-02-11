# Coupa Invoice Sync API (.NET 10 / C#)

This repository now includes a production-style Web API solution to synchronize invoices from Coupa, map them generically, route by region/destination system, persist to target databases, and stage failures as raw JSON payloads.

## Solution Structure

- `src/Coupa.InvoiceSync.Api`
  - `Controllers` – API endpoints.
  - `Application/Abstractions` – SOLID interfaces for integration points.
  - `Application/Services` – orchestration use case service.
  - `Application/Pipeline` – middleware-like processing pipeline abstractions and implementation.
  - `Domain/Entities` + `Domain/Enums` – business models.
  - `Infrastructure/Coupa` – Coupa API client.
  - `Infrastructure/Mapping` – generic field mapping middleware + validation middleware.
  - `Infrastructure/Persistence` – region/target repositories, repository factory, staging error repository.
  - `Infrastructure/Options` – typed config models.

## API Endpoint

- `POST /api/invoices/sync`
  - Calls Coupa Get Invoices API.
  - Maps source payload to canonical model with configurable middleware.
  - Routes records to target repository by `region` + `destination_system`.
  - Writes failed records to staging error table abstraction as raw JSON.
  - Returns success summary.

## Generic Mapping Middleware

Field mappings are configured in `appsettings.json` under `Mapping:Profiles:CoupaToCanonical:FieldMappings`.

Example:

```json
{ "SourceField": "supplier_name", "TargetField": "SupplierName" }
```

This enables easy source/target remapping without changing code.

## Region + Third-Party Routing

Routing is configured in `appsettings.json` under `Routing:RouteToRepository`.

Example:

```json
"NA:MSDynamics": "MsDynamicsInvoiceRepository"
```

This allows easy onboarding of additional third-party invoice sinks.

## Method/Class Summaries

All related classes and methods contain XML summaries so generated docs and IDE tooltips clearly explain responsibilities and behavior.

## Notes

- Project targets `.NET 10` as requested (`net10.0`).
- Repository implementations are in-memory placeholders to demonstrate clean architecture and extension points; replace with concrete SQL adapters in production.
