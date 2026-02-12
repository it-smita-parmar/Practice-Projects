using Coupa.Supplier.Application.DTOs;

namespace Coupa.Supplier.Application.Interfaces;

public interface ISupplierSyncService
{
    Task<SyncSuppliersResult> SyncAsync(SyncSuppliersRequest request, CancellationToken cancellationToken);
}
