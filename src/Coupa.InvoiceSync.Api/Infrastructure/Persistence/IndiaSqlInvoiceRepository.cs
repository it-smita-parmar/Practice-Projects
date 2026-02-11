using Coupa.InvoiceSync.Api.Application.Abstractions;
using Coupa.InvoiceSync.Api.Domain.Entities;

namespace Coupa.InvoiceSync.Api.Infrastructure.Persistence;

/// <summary>
/// Persists India invoices to a region-specific target database.
/// </summary>
public sealed class IndiaSqlInvoiceRepository : IInvoiceRepository
{
    private static readonly List<CanonicalInvoice> Invoices = [];

    /// <inheritdoc />
    public string RouteKey => "IN:TargetDb";

    /// <inheritdoc />
    public Task InsertInvoiceAsync(CanonicalInvoice invoice, CancellationToken cancellationToken)
    {
        Invoices.Add(invoice);
        return Task.CompletedTask;
    }
}
