using Coupa.Supplier.Domain.IntegrationEngine.Models;

namespace Coupa.Supplier.Domain.IntegrationEngine.Abstractions;

public interface IIntegrationConfigProvider
{
    Task<IntegrationConfiguration> GetByNameAsync(string integrationName, CancellationToken cancellationToken);
}
