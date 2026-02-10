using CoupaInvoiceIngestion.Api.Domain.Entities;

namespace CoupaInvoiceIngestion.Api.Application.Abstractions;

public interface IInvoiceProcessingMiddleware
{
    Task InvokeAsync(Invoice invoice, Func<Task> next, CancellationToken cancellationToken);
}
