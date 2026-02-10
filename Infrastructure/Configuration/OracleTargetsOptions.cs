namespace CoupaInvoiceIngestion.Api.Infrastructure.Configuration;

public sealed class OracleTargetsOptions
{
    public const string SectionName = "OracleTargets";

    public Dictionary<string, string> Connections { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
