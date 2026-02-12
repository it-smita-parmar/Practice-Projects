using Coupa.Supplier.Domain.IntegrationEngine.Models;

namespace Coupa.Supplier.Domain.IntegrationEngine.Abstractions;

public interface ITargetProvider
{
    string TargetType { get; }
    Task InsertSingleAsync(TargetConfiguration targetConfig, Dictionary<string, TableBatchData> mappedData, CancellationToken cancellationToken);
    Task InsertBulkAsync(TargetConfiguration targetConfig, Dictionary<string, TableBatchData> mappedData, CancellationToken cancellationToken);
}
