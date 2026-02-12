using System.Text.Json;
using Coupa.Supplier.Application.Configuration;
using Microsoft.Extensions.Options;

namespace Coupa.Supplier.Infrastructure.Mapping;

public sealed class JsonMappingConfigProvider(IOptions<SyncOptions> options) : IMappingConfigProvider
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private MappingConfig? _cached;

    public async Task<MappingConfig> GetAsync(CancellationToken cancellationToken)
    {
        if (_cached is not null) return _cached;

        await using var stream = File.OpenRead(options.Value.MappingFilePath);
        _cached = await JsonSerializer.DeserializeAsync<MappingConfig>(stream, SerializerOptions, cancellationToken)
                  ?? throw new InvalidOperationException("Unable to deserialize mapping file.");

        return _cached;
    }
}
