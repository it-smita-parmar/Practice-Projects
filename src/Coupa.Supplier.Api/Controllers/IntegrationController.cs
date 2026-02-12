using Coupa.Supplier.Domain.IntegrationEngine.Abstractions;
using Coupa.Supplier.Domain.IntegrationEngine.Models;
using Microsoft.AspNetCore.Mvc;

namespace Coupa.Supplier.Api.Controllers;

[ApiController]
[Route("api/integration")]
public sealed class IntegrationController(IIntegrationOrchestrator orchestrator) : ControllerBase
{
    [HttpPost("run/{integrationName}")]
    [ProducesResponseType(typeof(IntegrationRunResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Run([FromRoute] string integrationName, CancellationToken cancellationToken)
    {
        var result = await orchestrator.RunAsync(integrationName, cancellationToken);
        return Ok(result);
    }
}
