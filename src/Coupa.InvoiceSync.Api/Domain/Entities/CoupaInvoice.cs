using System.Text.Json;

namespace Coupa.InvoiceSync.Api.Domain.Entities;

/// <summary>
/// Represents a raw invoice payload returned by Coupa's Get Invoices API.
/// </summary>
public sealed class CoupaInvoice
{
    /// <summary>
    /// Gets or sets the Coupa invoice identifier.
    /// </summary>
    public required string InvoiceId { get; init; }

    /// <summary>
    /// Gets or sets the region that determines the destination target database.
    /// </summary>
    public required string Region { get; init; }

    /// <summary>
    /// Gets or sets the source invoice type.
    /// </summary>
    public required string InvoiceType { get; init; }

    /// <summary>
    /// Gets or sets the downstream destination system key such as "TargetDb" or "MSDynamics".
    /// </summary>
    public string DestinationSystem { get; init; } = "TargetDb";

    /// <summary>
    /// Gets or sets the full source payload as a JSON object.
    /// </summary>
    public required JsonElement Payload { get; init; }
}
