using CoupaInvoiceIngestion.Api.Application.Abstractions;
using CoupaInvoiceIngestion.Api.Domain.Entities;

namespace CoupaInvoiceIngestion.Api.Infrastructure.Persistence.Oracle;

public sealed class DynamicsInvoiceSink(
    string name,
    string profileName,
    IInvoiceMappingEngine mappingEngine,
    IInvoiceMappedPayloadStore mappedPayloadStore,
    ILogger<DynamicsInvoiceSink> logger) : IInvoiceSink
{
    public string Name => name;
    public string ProfileName => profileName;

    public Task PersistAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        var payload = mappedPayloadStore.Get(invoice.InvoiceId, Name) ?? mappingEngine.Map(invoice, profileName);
        logger.LogInformation("Mapped invoice {InvoiceId} to Dynamics payload with {Count} fields.", invoice.InvoiceId, payload.Count);
        return Task.CompletedTask;
    }
}
