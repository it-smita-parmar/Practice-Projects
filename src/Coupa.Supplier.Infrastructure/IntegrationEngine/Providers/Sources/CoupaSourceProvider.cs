using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Coupa.Supplier.Domain.IntegrationEngine.Abstractions;
using Coupa.Supplier.Domain.IntegrationEngine.Models;

namespace Coupa.Supplier.Infrastructure.IntegrationEngine.Providers.Sources;

public sealed class CoupaSourceProvider(IHttpClientFactory httpClientFactory) : ISourceProvider
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNameCaseInsensitive = true };
    public string SourceType => "Coupa";

    public async Task<JsonArray> GetDataAsync(SourceConfiguration sourceConfig, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient(nameof(CoupaSourceProvider));
        ApplyHeaders(client, sourceConfig.Headers);

        var response = await client.GetAsync(sourceConfig.Endpoint, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var node = await JsonNode.ParseAsync(stream, cancellationToken: cancellationToken);
        return node as JsonArray ?? new JsonArray(node);
    }

    public async Task SendDataAsync(SourceConfiguration sourceConfig, JsonObject payload, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient(nameof(CoupaSourceProvider));
        ApplyHeaders(client, sourceConfig.Headers);

        var json = JsonSerializer.Serialize(payload, SerializerOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PutAsync(sourceConfig.Endpoint, content, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private static void ApplyHeaders(HttpClient client, IReadOnlyDictionary<string, string> headers)
    {
        foreach (var (key, value) in headers)
        {
            if (!client.DefaultRequestHeaders.Contains(key))
            {
                client.DefaultRequestHeaders.Add(key, value);
            }
        }
    }
}
