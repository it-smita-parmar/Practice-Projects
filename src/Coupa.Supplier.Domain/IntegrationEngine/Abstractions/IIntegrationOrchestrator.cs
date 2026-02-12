using Coupa.Supplier.Domain.IntegrationEngine.Models;

namespace Coupa.Supplier.Domain.IntegrationEngine.Abstractions;

public interface IIntegrationOrchestrator
{
    Task<IntegrationRunResult> RunAsync(string integrationName, CancellationToken cancellationToken);
}
