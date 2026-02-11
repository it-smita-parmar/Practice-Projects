# Coupa Invoice Ingestion API (.NET 10)

A simplified Web API that:
1. Calls Coupa Get Invoices API.
2. Maps source fields to target fields using configurable profiles.
3. Routes each invoice to one or more targets based on region/source.
4. Inserts mapped rows into Oracle staging DBs (and supports third-party targets such as Dynamics).
5. Returns success metrics in the API response.

## Endpoint
- `POST /api/invoices/sync`

## Simple architecture
- `CoupaClient`: fetches invoices from Coupa.
- `InvoiceMappingEngine`: generic profile-based mapping (`targetField -> sourcePath`).
- `InvoiceSyncService`: orchestrates fetch + route + map + persist.
- `ITargetPersister` implementations:
  - `OracleTargetPersister`
  - `DynamicsTargetPersister` (extension stub)

## Config-driven behavior
- `InvoiceMappings:Profiles`: reusable field mappings.
- `TargetRouting:Targets`: routes invoices by region/source and selects persister + mapping profile.
- `OracleTargets:Connections`: regional Oracle connection strings.

## Run
```bash
dotnet restore
dotnet run
```

> Requires .NET SDK with `net10.0` support.
