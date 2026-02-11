using CoupaInvoiceIngestion.Api.Application.Abstractions;
using CoupaInvoiceIngestion.Api.Contracts;
using CoupaInvoiceIngestion.Api.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace CoupaInvoiceIngestion.Api.Application.Services;

public sealed class InvoiceSyncService(
    ICoupaClient coupaClient,
    IInvoiceMappingEngine mappingEngine,
    IEnumerable<ITargetPersister> persisters,
    IOptions<TargetRoutingOptions> routingOptions) : IInvoiceSyncService
{
    private readonly IReadOnlyDictionary<string, ITargetPersister> _persisters = persisters
        .ToDictionary(x => x.Type, StringComparer.OrdinalIgnoreCase);

    private readonly TargetRoutingOptions _routing = routingOptions.Value;

    public async Task<SyncInvoicesResponse> SyncInvoicesAsync(CancellationToken cancellationToken)
    {
        var invoices = await coupaClient.GetInvoicesAsync(cancellationToken);
        var persisted = 0;
        var persistedByTarget = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var invoice in invoices)
        {
            var matchedTargets = _routing.Targets.Where(target =>
                (target.Regions.Count == 0 || target.Regions.Contains(invoice.Region.Value, StringComparer.OrdinalIgnoreCase)) &&
                (target.Sources.Count == 0 || target.Sources.Contains(invoice.SourceSystem, StringComparer.OrdinalIgnoreCase)));

            foreach (var target in matchedTargets)
            {
                if (!_persisters.TryGetValue(target.Type, out var persister))
                {
                    throw new InvalidOperationException($"No persister registered for target type '{target.Type}'.");
                }

                var payload = mappingEngine.Map(invoice, target.ProfileName);
                await persister.PersistAsync(target, payload, cancellationToken);

                persisted++;
                persistedByTarget[target.Name] = persistedByTarget.TryGetValue(target.Name, out var count) ? count + 1 : 1;
            }
        }

        return new SyncInvoicesResponse(
            invoices.Count,
            persisted,
            "Invoices were fetched from Coupa and inserted into configured staging targets.",
            persistedByTarget);
    }
}
