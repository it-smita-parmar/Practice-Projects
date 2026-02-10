namespace CoupaInvoiceIngestion.Api.Application.Abstractions;

public interface IInvoiceMappedPayloadStore
{
    void Set(string invoiceId, string targetName, Dictionary<string, object?> payload);
    Dictionary<string, object?>? Get(string invoiceId, string targetName);
}
