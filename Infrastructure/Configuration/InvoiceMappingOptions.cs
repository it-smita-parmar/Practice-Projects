namespace CoupaInvoiceIngestion.Api.Infrastructure.Configuration;

public sealed class InvoiceMappingOptions
{
    public const string SectionName = "InvoiceMappings";

    public Dictionary<string, Dictionary<string, string>> Profiles { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
