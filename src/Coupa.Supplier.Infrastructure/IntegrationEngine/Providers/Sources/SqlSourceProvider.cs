using System.Data;
using System.Text.Json.Nodes;
using Coupa.Supplier.Domain.IntegrationEngine.Abstractions;
using Coupa.Supplier.Domain.IntegrationEngine.Models;
using Microsoft.Data.SqlClient;

namespace Coupa.Supplier.Infrastructure.IntegrationEngine.Providers.Sources;

public sealed class SqlSourceProvider : ISourceProvider
{
    public string SourceType => "SqlServer";

    public async Task<JsonArray> GetDataAsync(SourceConfiguration sourceConfig, CancellationToken cancellationToken)
    {
        var result = new JsonArray();

        await using var connection = new SqlConnection(sourceConfig.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sourceConfig.Query, connection) { CommandType = CommandType.Text };
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var row = new JsonObject();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : JsonValue.Create(reader.GetValue(i));
            }

            result.Add(row);
        }

        return result;
    }

    public Task SendDataAsync(SourceConfiguration sourceConfig, JsonObject payload, CancellationToken cancellationToken)
        => Task.CompletedTask;
}
