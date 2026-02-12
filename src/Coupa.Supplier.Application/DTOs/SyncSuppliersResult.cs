namespace Coupa.Supplier.Application.DTOs;

public sealed record SyncSuppliersResult(int TotalReceived, int TotalInserted, int TotalFailed);
