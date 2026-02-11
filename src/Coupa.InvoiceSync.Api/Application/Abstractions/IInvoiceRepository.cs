using Coupa.InvoiceSync.Api.Domain.Entities;

namespace Coupa.InvoiceSync.Api.Application.Abstractions;

/// <summary>
/// Persists canonical invoices into a region-specific target database.
/// </summary>
public interface IInvoiceRepository
{
    /// <summary>
    /// Gets the routing key used for selecting this repository (for example: "NA:TargetDb").
    /// </summary>
    string RouteKey { get; }

    /// <summary>
    /// Inserts the provided invoice into its target schema.
    /// </summary>
    /// <param name="invoice">The normalized invoice instance.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    Task InsertInvoiceAsync(CanonicalInvoice invoice, CancellationToken cancellationToken);
}
