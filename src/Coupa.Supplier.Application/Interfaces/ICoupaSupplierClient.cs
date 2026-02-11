using Coupa.Supplier.Domain.Entities;

namespace Coupa.Supplier.Application.Interfaces;

public interface ICoupaSupplierClient
{
    Task<IReadOnlyCollection<SupplierAggregate>> GetSuppliersAsync(CancellationToken cancellationToken);
}
