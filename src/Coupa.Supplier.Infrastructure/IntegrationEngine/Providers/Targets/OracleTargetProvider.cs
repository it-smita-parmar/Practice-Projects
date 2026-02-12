using Coupa.Supplier.Domain.IntegrationEngine.Abstractions;
using Coupa.Supplier.Domain.IntegrationEngine.Models;
using Coupa.Supplier.Infrastructure.IntegrationEngine.Persistence;
using Oracle.ManagedDataAccess.Client;

namespace Coupa.Supplier.Infrastructure.IntegrationEngine.Providers.Targets;

public sealed class OracleTargetProvider : ITargetProvider
{
    public string TargetType => "OracleIndia";

    public Task InsertSingleAsync(TargetConfiguration targetConfig, Dictionary<string, TableBatchData> mappedData, CancellationToken cancellationToken)
        => InsertAsync(targetConfig.ConnectionString, mappedData, false, cancellationToken);

    public Task InsertBulkAsync(TargetConfiguration targetConfig, Dictionary<string, TableBatchData> mappedData, CancellationToken cancellationToken)
        => InsertAsync(targetConfig.ConnectionString, mappedData, true, cancellationToken);

    private static async Task InsertAsync(string connectionString, Dictionary<string, TableBatchData> mappedData, bool bulk, CancellationToken cancellationToken)
    {
        await using var connection = new OracleConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        foreach (var (_, table) in mappedData)
        {
            if (table.RowCount == 0)
            {
                continue;
            }

            await using var command = connection.CreateCommand();
            command.BindByName = true;
            command.CommandText = DynamicSqlBuilder.BuildInsert(table.TableName, table.ColumnData.Keys.ToArray(), ":");

            if (bulk)
            {
                command.ArrayBindCount = table.RowCount;
            }

            foreach (var (column, values) in table.ColumnData)
            {
                command.Parameters.Add($":{column}", values.ToArray());
            }

            _ = await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
