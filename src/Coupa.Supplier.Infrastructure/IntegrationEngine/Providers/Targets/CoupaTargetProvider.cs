using System.Text;
using System.Text.Json;
using Coupa.Supplier.Domain.IntegrationEngine.Abstractions;
using Coupa.Supplier.Domain.IntegrationEngine.Models;

namespace Coupa.Supplier.Infrastructure.IntegrationEngine.Providers.Targets;

public sealed class CoupaTargetProvider(IHttpClientFactory httpClientFactory) : ITargetProvider
{
    public string TargetType => "Coupa";

    public async Task InsertSingleAsync(TargetConfiguration targetConfig, Dictionary<string, TableBatchData> mappedData, CancellationToken cancellationToken)
    {
        var client = BuildClient(targetConfig);
        foreach (var (_, table) in mappedData)
        {
            for (var rowIndex = 0; rowIndex < table.RowCount; rowIndex++)
            {
                var payload = table.ColumnData.ToDictionary(x => x.Key, x => x.Value[rowIndex]);
                var requestBody = JsonSerializer.Serialize(payload);
                using var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                var response = await client.PutAsync(targetConfig.Endpoint, content, cancellationToken);
                response.EnsureSuccessStatusCode();
            }
        }
    }

    public async Task InsertBulkAsync(TargetConfiguration targetConfig, Dictionary<string, TableBatchData> mappedData, CancellationToken cancellationToken)
    {
        var client = BuildClient(targetConfig);
        var body = JsonSerializer.Serialize(mappedData);
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        var response = await client.PutAsync(targetConfig.Endpoint, content, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private HttpClient BuildClient(TargetConfiguration targetConfig)
    {
        var client = httpClientFactory.CreateClient(nameof(CoupaTargetProvider));
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
