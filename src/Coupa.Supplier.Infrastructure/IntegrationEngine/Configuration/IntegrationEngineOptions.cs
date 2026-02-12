namespace Coupa.Supplier.Infrastructure.IntegrationEngine.Configuration;

public sealed class IntegrationEngineOptions
{
    public const string SectionName = "IntegrationEngine";
    public string ConfigRootPath { get; set; } = "config/integrations";
    public string ErrorLogConnectionString { get; set; } = string.Empty;
}
