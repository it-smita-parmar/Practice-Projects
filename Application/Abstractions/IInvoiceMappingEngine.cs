using CoupaInvoiceIngestion.Api.Domain.Entities;

namespace CoupaInvoiceIngestion.Api.Application.Abstractions;

public interface IInvoiceMappingEngine
{
    Dictionary<string, object?> Map(Invoice invoice, string profileName);
}
