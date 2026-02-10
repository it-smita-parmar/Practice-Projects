namespace CoupaInvoiceIngestion.Api.Contracts;

public sealed record SyncInvoicesResponse(
    int TotalInvoices,
    int PersistedInvoices,
    string Message,
    IReadOnlyDictionary<string, int> PersistedByTarget);
