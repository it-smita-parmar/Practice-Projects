using Coupa.Supplier.Domain.IntegrationEngine.Abstractions;
using Coupa.Supplier.Domain.IntegrationEngine.Models;

namespace Coupa.Supplier.Infrastructure.IntegrationEngine.Providers.Targets;

public sealed class OracleUsTargetProvider : ITargetProvider
{
    private readonly OracleTargetProvider _inner = new();
    public string TargetType => "OracleUS";

    public Task InsertSingleAsync(TargetConfiguration targetConfig, Dictionary<string, TableBatchData> mappedData, CancellationToken cancellationToken)
        => _inner.InsertSingleAsync(targetConfig, mappedData, cancellationToken);

    public Task InsertBulkAsync(TargetConfiguration targetConfig, Dictionary<string, TableBatchData> mappedData, CancellationToken cancellationToken)
        => _inner.InsertBulkAsync(targetConfig, mappedData, cancellationToken);
}
