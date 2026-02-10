using CoupaInvoiceIngestion.Api.Application.Abstractions;
using CoupaInvoiceIngestion.Api.Contracts;

namespace CoupaInvoiceIngestion.Api.Application.Services;

public sealed class InvoiceSyncService(
    ICoupaClient coupaClient,
    IInvoiceSinkRouter sinkRouter,
    IEnumerable<IInvoiceProcessingMiddleware> middlewares) : IInvoiceSyncService
{
    private readonly IReadOnlyList<IInvoiceProcessingMiddleware> _middlewares = middlewares.ToList();

    public async Task<SyncInvoicesResponse> SyncInvoicesAsync(CancellationToken cancellationToken)
    {
        var invoices = await coupaClient.GetInvoicesAsync(cancellationToken);
        var persisted = 0;
        var persistedByTarget = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var invoice in invoices)
        {
            await ExecutePipelineAsync(invoice, cancellationToken);

            var sinks = sinkRouter.ResolveSinks(invoice);
            foreach (var sink in sinks)
            {
                await sink.PersistAsync(invoice, cancellationToken);
                persisted++;
                persistedByTarget[sink.Name] = persistedByTarget.TryGetValue(sink.Name, out var count) ? count + 1 : 1;
            }
        }

        return new SyncInvoicesResponse(
            invoices.Count,
            persisted,
            "Invoices were fetched from Coupa and inserted into configured staging targets.",
            persistedByTarget);
    }

    private Task ExecutePipelineAsync(Domain.Entities.Invoice invoice, CancellationToken cancellationToken)
    {
        var index = -1;
        Task Next()
        {
            index++;
            if (index >= _middlewares.Count)
            {
                return Task.CompletedTask;
            }

            return _middlewares[index].InvokeAsync(invoice, Next, cancellationToken);
        }

        return Next();
    }
}
