using Coupa.Supplier.Domain.Entities;

namespace Coupa.Supplier.Application.Interfaces;

public interface ITargetSupplierRepository
{
    string TargetName { get; }
    Task<int> InsertBatchAsync(IReadOnlyCollection<SupplierAggregate> suppliers, bool useBulkInsert, CancellationToken cancellationToken);
    Task LogErrorAsync(string source, string rawJson, string errorMessage, CancellationToken cancellationToken);
}
