using Coupa.InvoiceSync.Api.Application.Pipeline;
using Coupa.InvoiceSync.Api.Domain.Enums;

namespace Coupa.InvoiceSync.Api.Infrastructure.Mapping;

/// <summary>
/// Validates mandatory fields after mapping and blocks unsupported invoice content.
/// </summary>
public sealed class InvoiceTypeValidationMiddleware : IInvoiceProcessingMiddleware
{
    /// <inheritdoc />
    public Task InvokeAsync(InvoiceProcessingContext context, Func<Task> next, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(context.CanonicalInvoice.InvoiceNumber))
        {
            throw new InvalidOperationException("InvoiceNumber is required after mapping.");
        }

        if (context.CanonicalInvoice.Category == InvoiceCategory.Unknown)
        {
            throw new InvalidOperationException("Invoice type is unsupported for processing.");
        }

        return next();
    }
}
