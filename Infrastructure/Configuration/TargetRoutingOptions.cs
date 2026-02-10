namespace CoupaInvoiceIngestion.Api.Infrastructure.Configuration;

public sealed class TargetRoutingOptions
{
    public const string SectionName = "TargetRouting";

    public List<TargetDefinition> Targets { get; init; } = [];
}

public sealed class TargetDefinition
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string ProfileName { get; init; } = string.Empty;
    public string TableName { get; init; } = "STAGING_INVOICES";
    public List<string> Regions { get; init; } = [];
    public List<string> Sources { get; init; } = [];
}
