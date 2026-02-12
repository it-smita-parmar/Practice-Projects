using System.Text.Json;
using System.Text.Json.Nodes;
using Coupa.Supplier.Domain.IntegrationEngine.Models;
using Coupa.Supplier.Infrastructure.IntegrationEngine.Mapping;
using Coupa.Supplier.Infrastructure.IntegrationEngine.Persistence;
using FluentAssertions;
using Xunit;

namespace Coupa.Supplier.Infrastructure.Tests.Mapping;

public sealed class MetadataMappingEngineTests
{
    [Fact]
    public async Task InboundConfig_Should_Contain_All_Supplier_Child_Tables()
    {
        var path = Path.GetFullPath("config/integrations/coupa-supplier-inbound.json");
        await using var stream = File.OpenRead(path);
        var config = await JsonSerializer.DeserializeAsync<IntegrationConfiguration>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        config.Should().NotBeNull();
        config!.Mapping.Tables.Select(t => t.TableName).Should().BeEquivalentTo(
            "xxfin.xxfin_coupa_suppliers",
            "xxfin.xxfin_coupa_supp_sites",
            "xxfin.xxfin_coupa_supp_contacts",
            "xxfin.xxfin_coupa_supp_adds",
            "xxfin.xxfin_coupa_supp_remit_to_adds");
    }

    [Fact]
    public async Task Transform_Should_Map_Parent_And_Child_Records_For_All_Tables()
    {
        var path = Path.GetFullPath("config/integrations/coupa-supplier-inbound.json");
        await using var stream = File.OpenRead(path);
        var config = await JsonSerializer.DeserializeAsync<IntegrationConfiguration>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var source = JsonNode.Parse("""
        {
          "id": 1001,
          "name": "Acme Corp",
          "status": "active",
          "supplier-sites": [
            { "id": 2001, "code": "HQ", "name": "Head Office", "po-method": "email", "po-change-method": "portal", "active": true }
          ],
          "contacts": [
            { "id": 3001 }
          ],
          "supplier-addresses": [
            { "id": 4001, "location-code": "LOC-1", "state-iso-code": "KA", "active": true }
          ],
          "remit-to-addresses": [
            { "id": 5001, "remit-to-code": "R1", "name": "Remit1", "street1": "line1", "street2": "line2", "city": "Bangalore", "state": "KA", "postal-code": "560001", "country": { "code": "IN", "name": "India" }, "active": true }
          ]
        }
        """);

        var engine = new MetadataMappingEngine(new InMemorySequenceGenerator());
        var result = engine.Transform(source!, config!.Mapping);

        result.Should().ContainKey("xxfin.xxfin_coupa_suppliers");
        result.Should().ContainKey("xxfin.xxfin_coupa_supp_sites");
        result.Should().ContainKey("xxfin.xxfin_coupa_supp_contacts");
        result.Should().ContainKey("xxfin.xxfin_coupa_supp_adds");
        result.Should().ContainKey("xxfin.xxfin_coupa_supp_remit_to_adds");

        var parentKey = result["xxfin.xxfin_coupa_suppliers"].ColumnData["STG_ID"][0];
        result["xxfin.xxfin_coupa_supp_sites"].ColumnData["STG_ID"][0].Should().Be(parentKey);
        result["xxfin.xxfin_coupa_supp_contacts"].ColumnData["STG_ID"][0].Should().Be(parentKey);
        result["xxfin.xxfin_coupa_supp_adds"].ColumnData["STG_ID"][0].Should().Be(parentKey);
        result["xxfin.xxfin_coupa_supp_remit_to_adds"].ColumnData["STG_ID"][0].Should().Be(parentKey);
    }
}
