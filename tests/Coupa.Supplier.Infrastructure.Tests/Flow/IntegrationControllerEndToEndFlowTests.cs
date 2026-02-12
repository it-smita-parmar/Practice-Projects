using System.Text.Json.Nodes;
using Coupa.Supplier.Api.Controllers;
using Coupa.Supplier.Application.IntegrationEngine;
using Coupa.Supplier.Domain.IntegrationEngine.Abstractions;
using Coupa.Supplier.Domain.IntegrationEngine.Models;
using Coupa.Supplier.Infrastructure.IntegrationEngine.Factories;
using Coupa.Supplier.Infrastructure.IntegrationEngine.Mapping;
using Coupa.Supplier.Infrastructure.IntegrationEngine.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Coupa.Supplier.Infrastructure.Tests.Flow;

public sealed class IntegrationControllerEndToEndFlowTests
{
    [Fact]
    public async Task Run_Should_Map_Sample_Json_And_Use_Dynamic_Bulk_Target_Without_Db_Call()
    {
        var sourcePayload = JsonNode.Parse("""
        {
          "id": 8,
          "name": "Bright Technologies Inc.",
          "status": "active",
          "supplier-sites": [
            { "id": 1, "code": "SF_CODE", "name": "San Francisco", "po-method": "prompt", "po-change-method": "prompt", "active": true }
          ]
        }
        """)!;

        var configProvider = new Mock<IIntegrationConfigProvider>();
        configProvider
            .Setup(x => x.GetByNameAsync("coupa-supplier-inbound", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IntegrationConfiguration
            {
                IntegrationName = "coupa-supplier-inbound",
                ProcessingMode = "Bulk",
                BatchSize = 100,
                DegreeOfParallelism = 1,
                Source = new SourceConfiguration { Type = "Coupa" },
                Target = new TargetConfiguration { Type = "OracleIndia" },
                Mapping = BuildSupplierMapping()
            });

        var coupaSource = new StubSourceProvider("Coupa", [sourcePayload]);
        var sqlSource = new StubSourceProvider("Sql", []);
        var oracleTarget = new CapturingTargetProvider("OracleIndia");
        var sqlTarget = new CapturingTargetProvider("SqlServer");

        var orchestrator = new IntegrationOrchestrator(
            configProvider.Object,
            new SourceProviderFactory([coupaSource, sqlSource]),
            new TargetProviderFactory([oracleTarget, sqlTarget]),
            new MetadataMappingEngine(new InMemorySequenceGenerator()),
            Mock.Of<IErrorLogService>(),
            Mock.Of<ILogger<IntegrationOrchestrator>>());

        var controller = new IntegrationController(orchestrator);

        var result = await controller.Run("coupa-supplier-inbound", CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = ok.Value.Should().BeOfType<IntegrationRunResult>().Subject;
        payload.TotalReceived.Should().Be(1);
        payload.TotalSucceeded.Should().Be(1);
        payload.TotalFailed.Should().Be(0);

        oracleTarget.BulkInsertCalls.Should().Be(1);
        oracleTarget.SingleInsertCalls.Should().Be(0);
        sqlTarget.BulkInsertCalls.Should().Be(0);
        sqlTarget.SingleInsertCalls.Should().Be(0);

        oracleTarget.LastMappedData.Should().NotBeNull();
        var mapped = oracleTarget.LastMappedData!;
        mapped["xxfin.xxfin_coupa_suppliers"].ColumnData["NAME"][0].Should().Be("BRIGHT TECHNOLOGIES INC.");
        mapped["xxfin.xxfin_coupa_suppliers"].ColumnData["SUPPLIER_ID"][0].Should().Be(8L);
        mapped["xxfin.xxfin_coupa_supp_sites"].ColumnData["SITE_ID"][0].Should().Be(1L);
        mapped["xxfin.xxfin_coupa_supp_sites"].ColumnData["STG_ID"][0].Should().Be(1L);
    }

    [Fact]
    public async Task RunOutbound_Should_Map_Sample_Json_And_Use_Dynamic_Single_Target_Without_Db_Call()
    {
        var sourcePayload = JsonNode.Parse("""{ "id": 42, "name": "Contoso" }""")!;

        var configProvider = new Mock<IIntegrationConfigProvider>();
        configProvider
            .Setup(x => x.GetByNameAsync("coupa-supplier-outbound", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IntegrationConfiguration
            {
                IntegrationName = "coupa-supplier-outbound",
                ProcessingMode = "Single",
                BatchSize = 100,
                DegreeOfParallelism = 1,
                Source = new SourceConfiguration { Type = "Coupa" },
                Target = new TargetConfiguration { Type = "SqlServer" },
                Mapping = BuildSupplierMapping()
            });

        var coupaSource = new StubSourceProvider("Coupa", [sourcePayload]);
        var oracleTarget = new CapturingTargetProvider("OracleIndia");
        var sqlTarget = new CapturingTargetProvider("SqlServer");

        var orchestrator = new IntegrationOrchestrator(
            configProvider.Object,
            new SourceProviderFactory([coupaSource]),
            new TargetProviderFactory([oracleTarget, sqlTarget]),
            new MetadataMappingEngine(new InMemorySequenceGenerator()),
            Mock.Of<IErrorLogService>(),
            Mock.Of<ILogger<IntegrationOrchestrator>>());

        var controller = new IntegrationController(orchestrator);

        var result = await controller.RunOutbound("coupa-supplier-outbound", CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = ok.Value.Should().BeOfType<IntegrationRunResult>().Subject;
        payload.TotalReceived.Should().Be(1);
        payload.TotalSucceeded.Should().Be(1);
        payload.TotalFailed.Should().Be(0);

        sqlTarget.SingleInsertCalls.Should().Be(1);
        sqlTarget.BulkInsertCalls.Should().Be(0);
        oracleTarget.SingleInsertCalls.Should().Be(0);
        oracleTarget.BulkInsertCalls.Should().Be(0);
    }

    private static MappingConfiguration BuildSupplierMapping()
        => new()
        {
            Tables =
            [
                new TableMappingConfiguration
                {
                    TableName = "xxfin.xxfin_coupa_suppliers",
                    Alias = "SUPPLIERS",
                    CollectionPath = "$",
                    PrimaryKey = "STG_ID",
                    GeneratePrimaryKey = "sequence(XXFIN_SUPP_STG_SEQ)",
                    Fields =
                    [
                        new FieldMappingConfiguration { SourcePath = "$.id", TargetColumn = "SUPPLIER_ID", IsRequired = true, DataType = "number" },
                        new FieldMappingConfiguration { SourcePath = "upper($.name)", TargetColumn = "NAME", IsRequired = true, DataType = "string" }
                    ]
                },
                new TableMappingConfiguration
                {
                    TableName = "xxfin.xxfin_coupa_supp_sites",
                    Alias = "SUPPLIER_SITES",
                    CollectionPath = "$.supplier-sites[*]",
                    ParentAlias = "SUPPLIERS",
                    ParentKeyColumn = "STG_ID",
                    Fields =
                    [
                        new FieldMappingConfiguration { SourcePath = "$.id", TargetColumn = "SITE_ID", IsRequired = true, DataType = "number" }
                    ]
                }
            ]
        };

    private sealed class StubSourceProvider(string sourceType, JsonArray payload) : ISourceProvider
    {
        public string SourceType { get; } = sourceType;

        public Task<JsonArray> GetDataAsync(SourceConfiguration sourceConfig, CancellationToken cancellationToken)
            => Task.FromResult(payload);

        public Task SendDataAsync(SourceConfiguration sourceConfig, JsonObject payload, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    private sealed class CapturingTargetProvider(string targetType) : ITargetProvider
    {
        public string TargetType { get; } = targetType;
        public int SingleInsertCalls { get; private set; }
        public int BulkInsertCalls { get; private set; }
        public Dictionary<string, TableBatchData>? LastMappedData { get; private set; }

        public Task InsertSingleAsync(TargetConfiguration targetConfig, Dictionary<string, TableBatchData> mappedData, CancellationToken cancellationToken)
        {
            SingleInsertCalls++;
            LastMappedData = mappedData;
            return Task.CompletedTask;
        }

        public Task InsertBulkAsync(TargetConfiguration targetConfig, Dictionary<string, TableBatchData> mappedData, CancellationToken cancellationToken)
        {
            BulkInsertCalls++;
            LastMappedData = mappedData;
            return Task.CompletedTask;
        }
    }
}
