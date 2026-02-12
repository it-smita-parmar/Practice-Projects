using System.Text.Json;
using Coupa.Supplier.Domain.IntegrationEngine.Abstractions;
using Coupa.Supplier.Domain.IntegrationEngine.Models;
using Microsoft.Extensions.Options;

namespace Coupa.Supplier.Infrastructure.IntegrationEngine.Configuration;

public sealed class FileIntegrationConfigProvider(IOptions<IntegrationEngineOptions> options) : IIntegrationConfigProvider
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<IntegrationConfiguration> GetByNameAsync(string integrationName, CancellationToken cancellationToken)
    {
        var fullPath = Path.Combine(options.Value.ConfigRootPath, $"{integrationName}.json");
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Integration configuration '{integrationName}' was not found.", fullPath);
        }

        await using var stream = File.OpenRead(fullPath);
        return await JsonSerializer.DeserializeAsync<IntegrationConfiguration>(stream, SerializerOptions, cancellationToken)
               ?? throw new InvalidOperationException($"Unable to deserialize integration config '{integrationName}'.");
    }
}
