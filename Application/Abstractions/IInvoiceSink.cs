using CoupaInvoiceIngestion.Api.Domain.Entities;

namespace CoupaInvoiceIngestion.Api.Application.Abstractions;

public interface IInvoiceSink
{
    string Name { get; }
    string ProfileName { get; }
    Task PersistAsync(Invoice invoice, CancellationToken cancellationToken);
}
