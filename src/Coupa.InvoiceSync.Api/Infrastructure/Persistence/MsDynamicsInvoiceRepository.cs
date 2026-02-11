using Coupa.InvoiceSync.Api.Application.Abstractions;
using Coupa.InvoiceSync.Api.Domain.Entities;

namespace Coupa.InvoiceSync.Api.Infrastructure.Persistence;

/// <summary>
/// Persists invoices for Microsoft Dynamics integration, demonstrating third-party target extensibility.
/// </summary>
public sealed class MsDynamicsInvoiceRepository : IInvoiceRepository
{
    private static readonly List<CanonicalInvoice> Invoices = [];

    /// <inheritdoc />
    public string RouteKey => "NA:MSDynamics";

    /// <inheritdoc />
    public Task InsertInvoiceAsync(CanonicalInvoice invoice, CancellationToken cancellationToken)
    {
        Invoices.Add(invoice);
        return Task.CompletedTask;
    }
}
