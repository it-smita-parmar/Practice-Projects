using Coupa.InvoiceSync.Api.Domain.Entities;

namespace Coupa.InvoiceSync.Api.Application.Abstractions;

/// <summary>
/// Orchestrates the end-to-end invoice synchronization flow.
/// </summary>
public interface IInvoiceSyncService
{
    /// <summary>
    /// Synchronizes all invoices from Coupa into the configured target databases.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>The summary of sync execution.</returns>
    Task<InvoiceSyncResult> SyncAsync(CancellationToken cancellationToken);
}
