using CoupaInvoiceIngestion.Api.Application.Abstractions;

namespace CoupaInvoiceIngestion.Api.Infrastructure.Mapping;

public sealed class InvoiceMappedPayloadStore : IInvoiceMappedPayloadStore
{
    private readonly Dictionary<string, Dictionary<string, Dictionary<string, object?>>> _store = new(StringComparer.OrdinalIgnoreCase);

    public void Set(string invoiceId, string targetName, Dictionary<string, object?> payload)
    {
        if (!_store.TryGetValue(invoiceId, out var targetMap))
        {
            targetMap = new Dictionary<string, Dictionary<string, object?>>(StringComparer.OrdinalIgnoreCase);
            _store[invoiceId] = targetMap;
        }

        targetMap[targetName] = payload;
    }

    public Dictionary<string, object?>? Get(string invoiceId, string targetName)
    {
        return _store.TryGetValue(invoiceId, out var targetMap) && targetMap.TryGetValue(targetName, out var payload)
            ? payload
            : null;
    }
}
