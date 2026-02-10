using CoupaInvoiceIngestion.Api.Application.Abstractions;
using CoupaInvoiceIngestion.Api.Domain.Entities;

namespace CoupaInvoiceIngestion.Api.Middleware;

public sealed class InvoiceMappingMiddleware(IInvoiceSinkRouter sinkRouter, IInvoiceMappingEngine mappingEngine, IInvoiceMappedPayloadStore payloadStore)
    : IInvoiceProcessingMiddleware
{
    public Task InvokeAsync(Invoice invoice, Func<Task> next, CancellationToken cancellationToken)
    {
        var sinks = sinkRouter.ResolveSinks(invoice);
        foreach (var sink in sinks)
        {
            var payload = mappingEngine.Map(invoice, sink.ProfileName);
            payloadStore.Set(invoice.InvoiceId, sink.Name, payload);
        }

        return next();
    }
}
