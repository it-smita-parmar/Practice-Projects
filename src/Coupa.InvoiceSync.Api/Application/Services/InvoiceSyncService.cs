using System.Text.Json;
using Coupa.InvoiceSync.Api.Application.Abstractions;
using Coupa.InvoiceSync.Api.Application.Pipeline;
using Coupa.InvoiceSync.Api.Domain.Entities;

namespace Coupa.InvoiceSync.Api.Application.Services;

/// <summary>
/// Coordinates invoice retrieval, mapping, routing, persistence, and staging error handling.
/// </summary>
public sealed class InvoiceSyncService(
    ICoupaInvoiceClient coupaInvoiceClient,
    IInvoiceProcessingPipeline processingPipeline,
    IInvoiceRepositoryFactory invoiceRepositoryFactory,
    IErrorStagingRepository errorStagingRepository) : IInvoiceSyncService
{
    /// <inheritdoc />
    public async Task<InvoiceSyncResult> SyncAsync(CancellationToken cancellationToken)
    {
        var invoices = await coupaInvoiceClient.GetInvoicesAsync(cancellationToken);
        var result = new InvoiceSyncResult();

        foreach (var invoice in invoices)
        {
            try
            {
                var context = new InvoiceProcessingContext
                {
                    SourceInvoice = invoice
                };

                await processingPipeline.ExecuteAsync(context, cancellationToken);

                var repository = invoiceRepositoryFactory.Resolve(context.CanonicalInvoice.Region, context.CanonicalInvoice.DestinationSystem);
                await repository.InsertInvoiceAsync(context.CanonicalInvoice, cancellationToken);
                result.Succeeded++;
            }
            catch (Exception exception)
            {
                var payload = JsonSerializer.Serialize(invoice.Payload);
                await errorStagingRepository.InsertErrorAsync(invoice.InvoiceId, payload, exception.Message, cancellationToken);
                result.Failed++;
            }
        }

        result.Message = result.Failed == 0
            ? $"Successfully inserted {result.Succeeded} invoices into target databases."
            : $"Inserted {result.Succeeded} invoices with {result.Failed} error records staged.";

        return result;
    }
}
