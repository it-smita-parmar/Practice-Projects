namespace Coupa.Supplier.Application.Configuration;

public sealed class CoupaOptions
{
    public const string SectionName = "Coupa";
    public string BaseUrl { get; set; } = string.Empty;
    public string SuppliersEndpoint { get; set; } = "/api/suppliers";
    public string? BearerToken { get; set; }
    public string? KeyVaultSecretName { get; set; }
}
