using Coupa.Supplier.Domain.IntegrationEngine.Models;

namespace Coupa.Supplier.Domain.IntegrationEngine.Abstractions;

public interface IErrorLogService
{
    Task LogAsync(RecordError error, CancellationToken cancellationToken);
}
