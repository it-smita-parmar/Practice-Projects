namespace Coupa.InvoiceSync.Api.Application.Pipeline;

/// <summary>
/// Defines middleware behavior for transforming and validating invoices.
/// </summary>
public interface IInvoiceProcessingMiddleware
{
    /// <summary>
    /// Executes middleware logic and delegates to the next component.
    /// </summary>
    /// <param name="context">The current processing context.</param>
    /// <param name="next">The callback to execute the next middleware.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    Task InvokeAsync(InvoiceProcessingContext context, Func<Task> next, CancellationToken cancellationToken);
}
