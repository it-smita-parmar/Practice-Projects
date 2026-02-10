using CoupaInvoiceIngestion.Api.Domain.Entities;

namespace CoupaInvoiceIngestion.Api.Application.Abstractions;

public interface IInvoiceSinkRouter
{
    IReadOnlyCollection<IInvoiceSink> ResolveSinks(Invoice invoice);
}
