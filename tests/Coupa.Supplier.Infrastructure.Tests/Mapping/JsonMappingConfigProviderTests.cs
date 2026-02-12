using Coupa.Supplier.Application.Configuration;
using Coupa.Supplier.Infrastructure.Mapping;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace Coupa.Supplier.Infrastructure.Tests.Mapping;

public sealed class JsonMappingConfigProviderTests
{
    [Fact]
    public async Task GetAsync_Should_Load_And_Cache_Mapping_Config()
    {
        var options = Options.Create(new SyncOptions { MappingFilePath = Path.GetFullPath("config/coupa-mapping.json") });
        var provider = new JsonMappingConfigProvider(options);

        var first = await provider.GetAsync(CancellationToken.None);
        var second = await provider.GetAsync(CancellationToken.None);

        first.Entity.Should().Be("CoupaSupplier");
        first.Tables.Should().NotBeEmpty();
        ReferenceEquals(first, second).Should().BeTrue();
    }
}
