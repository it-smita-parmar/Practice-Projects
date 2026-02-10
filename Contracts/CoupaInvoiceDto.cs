namespace CoupaInvoiceIngestion.Api.Contracts;

public sealed class CoupaInvoiceDto
{
    public string? Id { get; init; }
    public string? SupplierName { get; init; }
    public decimal Total { get; init; }
    public string? Currency { get; init; }
    public DateTime InvoiceDate { get; init; }
    public string? Category { get; init; }
    public string? Region { get; init; }
    public string? SourceSystem { get; init; }
    public Dictionary<string, object?> Metadata { get; init; } = [];
}
