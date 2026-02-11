using Coupa.InvoiceSync.Api.Application.Abstractions;
using Coupa.InvoiceSync.Api.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Coupa.InvoiceSync.Api.Controllers;

/// <summary>
/// Exposes API endpoints for Coupa invoice synchronization operations.
/// </summary>
[ApiController]
[Route("api/invoices")]
public sealed class InvoiceSyncController(IInvoiceSyncService invoiceSyncService) : ControllerBase
{
    /// <summary>
    /// Pulls invoices from Coupa, maps them, inserts them into target databases, and returns a sync summary.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>The synchronization result summary.</returns>
    [HttpPost("sync")]
    [ProducesResponseType(typeof(InvoiceSyncResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> SyncInvoices(CancellationToken cancellationToken)
    {
        var result = await invoiceSyncService.SyncAsync(cancellationToken);
        return Ok(result);
    }
}
