namespace Coupa.Supplier.Infrastructure.IntegrationEngine.Configuration;

public sealed class IntegrationEngineOptions
{
    public const string SectionName = "IntegrationEngine";
    public string ConfigRootPath { get; set; } = "config/integrations";
    public string ErrorLogConnectionString { get; set; } = string.Empty;
    public string CoupaBaseUrl { get; set; } = string.Empty;
    public string MsDynamicsBaseUrl { get; set; } = string.Empty;
    public Dictionary<string, string> ApiEndpoints { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
