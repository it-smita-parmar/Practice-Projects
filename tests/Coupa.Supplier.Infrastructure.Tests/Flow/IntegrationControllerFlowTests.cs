using Coupa.Supplier.Api.Controllers;
using Coupa.Supplier.Domain.IntegrationEngine.Abstractions;
using Coupa.Supplier.Domain.IntegrationEngine.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Coupa.Supplier.Infrastructure.Tests.Flow;

public sealed class IntegrationControllerFlowTests
{
    [Fact]
    public async Task Run_Should_Return_Ok_And_Call_Orchestrator_Without_Db_Call()
    {
        var orchestrator = new Mock<IIntegrationOrchestrator>();
        var expected = new IntegrationRunResult("coupa-supplier-inbound", 10, 8, 2, []);
        orchestrator
            .Setup(x => x.RunAsync("coupa-supplier-inbound", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var controller = new IntegrationController(orchestrator.Object);

        var result = await controller.Run("coupa-supplier-inbound", CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(expected);
        orchestrator.Verify(x => x.RunAsync("coupa-supplier-inbound", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RunOutbound_Should_Return_Ok_And_Call_Orchestrator_Without_Db_Call()
    {
        var orchestrator = new Mock<IIntegrationOrchestrator>();
        var expected = new IntegrationRunResult("coupa-supplier-outbound", 6, 6, 0, []);
        orchestrator
            .Setup(x => x.RunAsync("coupa-supplier-outbound", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var controller = new IntegrationController(orchestrator.Object);

        var result = await controller.RunOutbound("coupa-supplier-outbound", CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(expected);
        orchestrator.Verify(x => x.RunAsync("coupa-supplier-outbound", It.IsAny<CancellationToken>()), Times.Once);
    }
}
