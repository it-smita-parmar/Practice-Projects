using CoupaInvoiceIngestion.Api.Application.Abstractions;
using CoupaInvoiceIngestion.Api.Infrastructure.Configuration;

namespace CoupaInvoiceIngestion.Api.Infrastructure.Persistence.Oracle;

public sealed class DynamicsTargetPersister(ILogger<DynamicsTargetPersister> logger) : ITargetPersister
{
    public string Type => "dynamics";

    public Task PersistAsync(TargetDefinition target, Dictionary<string, object?> payload, CancellationToken cancellationToken)
    {
        logger.LogInformation("Mapped invoice payload with {Count} fields for target {TargetName}.", payload.Count, target.Name);
        return Task.CompletedTask;
    }
}
