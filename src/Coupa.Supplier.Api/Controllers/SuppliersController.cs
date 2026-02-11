using Coupa.Supplier.Application.DTOs;
using Coupa.Supplier.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Coupa.Supplier.Api.Controllers;

[ApiController]
[Route("api/suppliers")]
public sealed class SuppliersController(ISupplierSyncService supplierSyncService, IValidator<SyncSuppliersRequest> validator) : ControllerBase
{
    [HttpPost("sync")]
    [ProducesResponseType(typeof(SyncSuppliersResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Sync([FromBody] SyncSuppliersRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(detail: string.Join(";", validationResult.Errors.Select(x => x.ErrorMessage)));
        }

        var result = await supplierSyncService.SyncAsync(request, cancellationToken);
        return Ok(result);
    }
}
