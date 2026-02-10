using CoupaInvoiceIngestion.Api.Domain.Entities;

namespace CoupaInvoiceIngestion.Api.Application.Abstractions;

public interface ICoupaClient
{
    Task<IReadOnlyCollection<Invoice>> GetInvoicesAsync(CancellationToken cancellationToken);
}
