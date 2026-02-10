namespace CoupaInvoiceIngestion.Api.Domain.ValueObjects;

public readonly record struct InvoiceRegion(string Value)
{
    public static InvoiceRegion From(string region)
    {
        if (string.IsNullOrWhiteSpace(region))
        {
            throw new ArgumentException("Region is required.", nameof(region));
        }

        return new InvoiceRegion(region.Trim().ToUpperInvariant());
    }

    public override string ToString() => Value;
}
