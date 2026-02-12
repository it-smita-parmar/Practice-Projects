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
    public Task<IActionResult> Run([FromRoute] string integrationName, CancellationToken cancellationToken)
        => RunIntegrationAsync(integrationName, cancellationToken);

    [HttpPost("outbound/{integrationName}")]
    [ProducesResponseType(typeof(IntegrationRunResult), StatusCodes.Status200OK)]
    public Task<IActionResult> RunOutbound([FromRoute] string integrationName, CancellationToken cancellationToken)
        => RunIntegrationAsync(integrationName, cancellationToken);

    private async Task<IActionResult> RunIntegrationAsync(string integrationName, CancellationToken cancellationToken)
    {
        var result = await orchestrator.RunAsync(integrationName, cancellationToken);
        return Ok(result);
    }
}
