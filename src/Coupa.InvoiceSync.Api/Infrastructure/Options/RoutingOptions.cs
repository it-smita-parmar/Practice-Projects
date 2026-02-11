namespace Coupa.InvoiceSync.Api.Infrastructure.Options;

/// <summary>
/// Contains route keys used to map regions and systems to target repositories.
/// </summary>
public sealed class RoutingOptions
{
    /// <summary>
    /// Gets the configuration section name.
    /// </summary>
    public const string SectionName = "Routing";

    /// <summary>
    /// Gets or sets dictionary mapping route keys to repository keys.
    /// </summary>
    public Dictionary<string, string> RouteToRepository { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
