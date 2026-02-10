namespace CoupaInvoiceIngestion.Api.Contracts;

public sealed class CoupaInvoicesResponse
{
    public List<CoupaInvoiceDto> Invoices { get; init; } = [];
}
