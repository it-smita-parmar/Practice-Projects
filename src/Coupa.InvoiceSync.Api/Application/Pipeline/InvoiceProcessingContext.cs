using Coupa.InvoiceSync.Api.Domain.Entities;

namespace Coupa.InvoiceSync.Api.Application.Pipeline;

/// <summary>
/// Holds state that flows through the invoice processing middleware pipeline.
/// </summary>
public sealed class InvoiceProcessingContext
{
    /// <summary>
    /// Gets or sets the source Coupa invoice.
    /// </summary>
    public required CoupaInvoice SourceInvoice { get; init; }

    /// <summary>
    /// Gets or sets the mapped canonical invoice.
    /// </summary>
    public CanonicalInvoice CanonicalInvoice { get; set; } = new();
}
