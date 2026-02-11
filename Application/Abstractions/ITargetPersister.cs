using CoupaInvoiceIngestion.Api.Infrastructure.Configuration;

namespace CoupaInvoiceIngestion.Api.Application.Abstractions;

public interface ITargetPersister
{
    string Type { get; }
    Task PersistAsync(TargetDefinition target, Dictionary<string, object?> payload, CancellationToken cancellationToken);
}
