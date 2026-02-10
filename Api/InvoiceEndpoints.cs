using CoupaInvoiceIngestion.Api.Application.Abstractions;

namespace CoupaInvoiceIngestion.Api.Api;

public static class InvoiceEndpoints
{
    public static IEndpointRouteBuilder MapInvoiceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/invoices/sync", async (IInvoiceSyncService service, CancellationToken cancellationToken) =>
        {
            var result = await service.SyncInvoicesAsync(cancellationToken);
            return Results.Ok(result);
        })
        .WithName("SyncCoupaInvoices")
        .WithSummary("Fetch Coupa invoices and persist them to regional staging targets.");

        return endpoints;
    }
}
