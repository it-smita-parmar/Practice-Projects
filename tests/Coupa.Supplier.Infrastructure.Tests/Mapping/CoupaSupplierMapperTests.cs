using System.Text.Json;
using Coupa.Supplier.Application.Configuration;
using Coupa.Supplier.Infrastructure.Mapping;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Coupa.Supplier.Infrastructure.Tests.Mapping;

public sealed class CoupaSupplierMapperTests
{
    [Fact]
    public async Task MapAsync_Should_Map_All_Configured_Fields_Without_Db_Call()
    {
        var options = Options.Create(new SyncOptions { MappingFilePath = Path.GetFullPath("config/coupa-mapping.json") });
        var provider = new JsonMappingConfigProvider(options);
        var mapper = new CoupaSupplierMapper(provider);

        using var json = JsonDocument.Parse("""
        {
          "id": 8,
          "name": "Bright Technologies Inc.",
          "status": "active",
          "supplier-sites": [
            { "id": 1, "code": "SF_CODE", "name": "San Francisco", "po-method": "prompt", "po-change-method": "prompt", "active": true }
          ],
          "contacts": [ { "id": 12 } ],
          "supplier-addresses": [ { "id": 17, "location-code": "LOC1", "state-iso-code": "US-CA", "active": true } ],
          "remit-to-addresses": [
            {
              "id": 18,
              "remit-to-code": "Bright Remit - USA",
              "name": "Bright Remit - USA",
              "street1": "400 Pine Street",
              "street2": "",
              "city": "San Francisco",
              "state": "CA",
              "postal-code": "94104",
              "active": true,
              "country": { "code": "US", "name": "United States" }
            }
          ]
        }
        """);

        var result = await mapper.MapAsync(json.RootElement, CancellationToken.None);

        result.SupplierId.Should().Be(8);
        result.Name.Should().Be("Bright Technologies Inc.");
        result.Status.Should().Be("active");
        result.Source.Should().Be("COUPA");

        result.Sites.Should().ContainSingle();
        result.Contacts.Should().ContainSingle();
        result.Addresses.Should().ContainSingle();
        result.RemitToAddresses.Should().ContainSingle();
        result.RemitToAddresses.Single().Country!.Code.Should().Be("US");
    }

    [Fact]
    public async Task MapAsync_Should_Default_Required_Fields_When_Missing()
    {
        var options = Options.Create(new SyncOptions { MappingFilePath = Path.GetFullPath("config/coupa-mapping.json") });
        var provider = new JsonMappingConfigProvider(options);
        var mapper = new CoupaSupplierMapper(provider);

        using var json = JsonDocument.Parse("""{ "status": "inactive" }""");

        var result = await mapper.MapAsync(json.RootElement, CancellationToken.None);

        result.SupplierId.Should().Be(0);
        result.Name.Should().BeEmpty();
        result.Status.Should().Be("inactive");
        result.Source.Should().Be("COUPA");
    }
}
