using Coupa.InvoiceSync.Api.Domain.Entities;

namespace Coupa.InvoiceSync.Api.Application.Abstractions;

/// <summary>
/// Provides access to Coupa invoice data.
/// </summary>
public interface ICoupaInvoiceClient
{
    /// <summary>
    /// Calls Coupa's Get Invoices API and returns all discovered invoices.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A list of source invoices.</returns>
    Task<IReadOnlyCollection<CoupaInvoice>> GetInvoicesAsync(CancellationToken cancellationToken);
}
