namespace Coupa.InvoiceSync.Api.Application.Pipeline;

/// <summary>
/// Executes registered middleware components using a chain-of-responsibility pipeline.
/// </summary>
public sealed class InvoiceProcessingPipeline(IEnumerable<IInvoiceProcessingMiddleware> middlewares) : IInvoiceProcessingPipeline
{
    private readonly IReadOnlyList<IInvoiceProcessingMiddleware> _middlewares = middlewares.ToList();

    /// <inheritdoc />
    public Task ExecuteAsync(InvoiceProcessingContext context, CancellationToken cancellationToken)
    {
        var index = -1;

        Task Next()
        {
            index++;
            if (index >= _middlewares.Count)
            {
                return Task.CompletedTask;
            }

            return _middlewares[index].InvokeAsync(context, Next, cancellationToken);
        }

        return Next();
    }
}
