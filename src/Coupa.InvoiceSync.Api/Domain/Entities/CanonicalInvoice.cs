using Coupa.InvoiceSync.Api.Domain.Enums;

namespace Coupa.InvoiceSync.Api.Domain.Entities;

/// <summary>
/// Represents a normalized invoice model used across mapping and persistence workflows.
/// </summary>
public sealed class CanonicalInvoice
{
    /// <summary>
    /// Gets or sets the external invoice identifier.
    /// </summary>
    public string InvoiceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the invoice number.
    /// </summary>
    public string InvoiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the invoice region.
    /// </summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the downstream target system.
    /// </summary>
    public string DestinationSystem { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the invoice supplier name.
    /// </summary>
    public string SupplierName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the invoice currency code.
    /// </summary>
    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the invoice amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the invoice issue date.
    /// </summary>
    public DateTimeOffset IssueDate { get; set; }

    /// <summary>
    /// Gets or sets the normalized invoice category.
    /// </summary>
    public InvoiceCategory Category { get; set; } = InvoiceCategory.Unknown;
}
