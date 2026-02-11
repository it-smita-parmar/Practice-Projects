using Coupa.InvoiceSync.Api.Application.Abstractions;
using Coupa.InvoiceSync.Api.Domain.Entities;

namespace Coupa.InvoiceSync.Api.Infrastructure.Persistence;

/// <summary>
/// Persists North America invoices to a region-specific target database.
/// </summary>
public sealed class NorthAmericaSqlInvoiceRepository : IInvoiceRepository
{
    private static readonly List<CanonicalInvoice> PurchaseInvoices = [];
    private static readonly List<CanonicalInvoice> LicensingInvoices = [];
    private static readonly List<CanonicalInvoice> ResourceInvoices = [];

    /// <inheritdoc />
    public string RouteKey => "NA:TargetDb";

    /// <inheritdoc />
    public Task InsertInvoiceAsync(CanonicalInvoice invoice, CancellationToken cancellationToken)
    {
        InsertByCategory(invoice);
        return Task.CompletedTask;
    }

    private static void InsertByCategory(CanonicalInvoice invoice)
    {
        switch (invoice.Category)
        {
            case Domain.Enums.InvoiceCategory.Purchase:
                PurchaseInvoices.Add(invoice);
                break;
            case Domain.Enums.InvoiceCategory.Licensing:
                LicensingInvoices.Add(invoice);
                break;
            case Domain.Enums.InvoiceCategory.ResourceBilling:
                ResourceInvoices.Add(invoice);
                break;
            default:
                throw new InvalidOperationException("Invoice category is not supported by the North America target database.");
        }
    }
}
