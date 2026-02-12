using Coupa.Supplier.Application.Interfaces;
using Coupa.Supplier.Domain.Entities;

namespace Coupa.Supplier.Infrastructure.Targets;

public sealed class SqlTargetRepository : ITargetSupplierRepository
{
    public string TargetName => "sql";

    public Task<int> InsertBatchAsync(IReadOnlyCollection<SupplierAggregate> suppliers, bool useBulkInsert, CancellationToken cancellationToken)
    {
        throw new NotImplementedException("SQL Server target is reserved for future extension.");
    }

    public Task LogErrorAsync(string source, string rawJson, string errorMessage, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
