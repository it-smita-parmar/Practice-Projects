namespace Coupa.InvoiceSync.Api.Application.Pipeline;

/// <summary>
/// Represents the composed middleware pipeline for invoice processing.
/// </summary>
public interface IInvoiceProcessingPipeline
{
    /// <summary>
    /// Executes all registered middlewares in order.
    /// </summary>
    /// <param name="context">The processing context.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    Task ExecuteAsync(InvoiceProcessingContext context, CancellationToken cancellationToken);
}
