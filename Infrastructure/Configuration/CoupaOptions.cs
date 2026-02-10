namespace CoupaInvoiceIngestion.Api.Infrastructure.Configuration;

public sealed class CoupaOptions
{
    public const string SectionName = "Coupa";

    public string BaseUrl { get; init; } = string.Empty;
    public string InvoicesPath { get; init; } = "/api/invoices";
    public string ApiKey { get; init; } = string.Empty;
}
