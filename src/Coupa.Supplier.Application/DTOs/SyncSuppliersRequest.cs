namespace Coupa.Supplier.Application.DTOs;

public sealed record SyncSuppliersRequest(
    string? Target = null,
    bool UseBulkInsert = true,
    int? BatchSize = null,
    int DegreeOfParallelism = 4);
