using Coupa.Supplier.Api.Controllers;
using Coupa.Supplier.Application.Abstractions;
using Coupa.Supplier.Application.Configuration;
using Coupa.Supplier.Application.DTOs;
using Coupa.Supplier.Application.Interfaces;
using Coupa.Supplier.Application.Services;
using Coupa.Supplier.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Coupa.Supplier.Infrastructure.Tests.Flow;

public sealed class SuppliersControllerFlowTests
{
    [Fact]
    public async Task Sync_Should_Return_Ok_And_Insert_Valid_Suppliers_Without_Db_Call()
    {
        var coupaClient = new Mock<ICoupaSupplierClient>();
        coupaClient
            .Setup(x => x.GetSuppliersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new SupplierAggregate { SupplierId = 10, Name = "Acme", RawJson = "{\"id\":10}" }
            ]);

        var repository = new Mock<ITargetSupplierRepository>();
        repository.SetupGet(x => x.TargetName).Returns("oracle");
        repository
            .Setup(x => x.InsertBatchAsync(It.IsAny<IReadOnlyCollection<SupplierAggregate>>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<SupplierAggregate> rows, bool _, CancellationToken _) => rows.Count);

        var strategy = new Mock<ITargetRepositoryStrategy>();
        strategy.Setup(x => x.Resolve(It.IsAny<string?>())).Returns(repository.Object);

        var service = CreateSupplierSyncService(coupaClient.Object, strategy.Object);
        var controller = new SuppliersController(service, new SupplierSyncRequestValidator());

        var result = await controller.Sync(new SyncSuppliersRequest(Target: "oracle", UseBulkInsert: true, BatchSize: 100, DegreeOfParallelism: 1), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(new SyncSuppliersResult(1, 1, 0));
        repository.Verify(x => x.InsertBatchAsync(It.IsAny<IReadOnlyCollection<SupplierAggregate>>(), true, It.IsAny<CancellationToken>()), Times.Once);
        repository.Verify(x => x.LogErrorAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Sync_Should_Log_Invalid_Suppliers_Without_Db_Call()
    {
        var coupaClient = new Mock<ICoupaSupplierClient>();
        coupaClient
            .Setup(x => x.GetSuppliersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new SupplierAggregate { SupplierId = 0, Name = "", RawJson = "{\"id\":0}", Source = "COUPA" }
            ]);

        var repository = new Mock<ITargetSupplierRepository>();
        repository.SetupGet(x => x.TargetName).Returns("oracle");
        repository.Setup(x => x.InsertBatchAsync(It.IsAny<IReadOnlyCollection<SupplierAggregate>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var strategy = new Mock<ITargetRepositoryStrategy>();
        strategy.Setup(x => x.Resolve(It.IsAny<string?>())).Returns(repository.Object);

        var service = CreateSupplierSyncService(coupaClient.Object, strategy.Object);
        var controller = new SuppliersController(service, new SupplierSyncRequestValidator());

        var result = await controller.Sync(new SyncSuppliersRequest(Target: "oracle", UseBulkInsert: true, BatchSize: 10, DegreeOfParallelism: 1), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(new SyncSuppliersResult(1, 0, 1));
        repository.Verify(x => x.InsertBatchAsync(It.IsAny<IReadOnlyCollection<SupplierAggregate>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        repository.Verify(x => x.LogErrorAsync("COUPA", "{\"id\":0}", It.Is<string>(m => m.Contains("Required fields missing")), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static SupplierSyncService CreateSupplierSyncService(ICoupaSupplierClient coupaClient, ITargetRepositoryStrategy strategy)
    {
        var scopeFactory = new Mock<IServiceScopeFactory>();
        var scope = new Mock<IServiceScope>();
        var provider = new Mock<IServiceProvider>();
        provider.Setup(x => x.GetService(typeof(ITargetRepositoryStrategy))).Returns(strategy);
        scope.SetupGet(x => x.ServiceProvider).Returns(provider.Object);
        scopeFactory.Setup(x => x.CreateScope()).Returns(scope.Object);

        return new SupplierSyncService(
            coupaClient,
            scopeFactory.Object,
            Mock.Of<ILogger<SupplierSyncService>>(),
            Options.Create(new SyncOptions { BatchSize = 50, DegreeOfParallelism = 1, UseBulkInsertDefault = false }));
    }
}
