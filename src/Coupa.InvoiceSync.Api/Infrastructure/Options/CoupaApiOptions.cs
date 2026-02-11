namespace Coupa.InvoiceSync.Api.Infrastructure.Options;

/// <summary>
/// Stores Coupa API connectivity settings.
/// </summary>
public sealed class CoupaApiOptions
{
    /// <summary>
    /// Gets the configuration section name.
    /// </summary>
    public const string SectionName = "CoupaApi";

    /// <summary>
    /// Gets or sets Coupa base URL.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets relative Get Invoices endpoint path.
    /// </summary>
    public string GetInvoicesPath { get; set; } = "/api/invoices";

    /// <summary>
    /// Gets or sets Coupa API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
}
