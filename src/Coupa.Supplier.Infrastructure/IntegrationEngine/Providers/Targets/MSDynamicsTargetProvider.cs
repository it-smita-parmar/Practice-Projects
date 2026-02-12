using System.Text;
using System.Text.Json;
using Coupa.Supplier.Domain.IntegrationEngine.Abstractions;
using Coupa.Supplier.Domain.IntegrationEngine.Models;
using Coupa.Supplier.Infrastructure.IntegrationEngine.Configuration;
using Microsoft.Extensions.Options;

namespace Coupa.Supplier.Infrastructure.IntegrationEngine.Providers.Targets;

public sealed class MSDynamicsTargetProvider(IHttpClientFactory httpClientFactory, IOptions<IntegrationEngineOptions> options) : ITargetProvider
{
    public string TargetType => "MSDynamics";

    public async Task InsertSingleAsync(TargetConfiguration targetConfig, Dictionary<string, TableBatchData> mappedData, CancellationToken cancellationToken)
    {
        var client = BuildClient(targetConfig);
        var endpoint = ResolveEndpoint(targetConfig.Endpoint);

        foreach (var (_, table) in mappedData)
        {
            for (var i = 0; i < table.RowCount; i++)
            {
                var payload = table.ColumnData.ToDictionary(x => x.Key, x => x.Value[i]);
                var json = JsonSerializer.Serialize(payload);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(endpoint, content, cancellationToken);
                response.EnsureSuccessStatusCode();
            }
        }
    }

    public async Task InsertBulkAsync(TargetConfiguration targetConfig, Dictionary<string, TableBatchData> mappedData, CancellationToken cancellationToken)
    {
        var client = BuildClient(targetConfig);
        var payload = JsonSerializer.Serialize(mappedData);
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(ResolveEndpoint(targetConfig.Endpoint), content, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private string ResolveEndpoint(string endpoint)
    {
        if (endpoint.StartsWith('@') && options.Value.ApiEndpoints.TryGetValue(endpoint[1..], out var resolved))
        {
            return resolved;
        }

        return endpoint;
    }

    private HttpClient BuildClient(TargetConfiguration targetConfig)
    {
        var client = httpClientFactory.CreateClient(nameof(MSDynamicsTargetProvider));
        foreach (var (key, value) in targetConfig.Headers)
        {
            if (!client.DefaultRequestHeaders.Contains(key))
            {
                client.DefaultRequestHeaders.Add(key, value);
            }
        }

        return client;
    }
}
