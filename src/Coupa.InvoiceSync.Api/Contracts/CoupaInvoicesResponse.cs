using System.Text.Json.Serialization;

namespace Coupa.InvoiceSync.Api.Contracts;

/// <summary>
/// Represents a simplified Coupa API response envelope.
/// </summary>
public sealed class CoupaInvoicesResponse
{
    /// <summary>
    /// Gets or sets invoice entries returned by Coupa.
    /// </summary>
    [JsonPropertyName("invoices")]
    public List<CoupaInvoiceRecord> Invoices { get; set; } = [];
}

/// <summary>
/// Represents a single invoice item returned from Coupa.
/// </summary>
public sealed class CoupaInvoiceRecord
{
    /// <summary>
    /// Gets or sets invoice id.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// Gets or sets region.
    /// </summary>
    [JsonPropertyName("region")]
    public required string Region { get; init; }

    /// <summary>
    /// Gets or sets invoice type.
    /// </summary>
    [JsonPropertyName("invoice_type")]
    public required string InvoiceType { get; init; }

    /// <summary>
    /// Gets or sets downstream destination system.
    /// </summary>
    [JsonPropertyName("destination_system")]
    public string DestinationSystem { get; init; } = "TargetDb";

    /// <summary>
    /// Gets or sets invoice payload.
    /// </summary>
    [JsonPropertyName("payload")]
    public required System.Text.Json.JsonElement Payload { get; init; }
}
