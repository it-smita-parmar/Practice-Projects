using CoupaInvoiceIngestion.Api.Contracts;

namespace CoupaInvoiceIngestion.Api.Application.Abstractions;

public interface IInvoiceSyncService
{
    Task<SyncInvoicesResponse> SyncInvoicesAsync(CancellationToken cancellationToken);
}
