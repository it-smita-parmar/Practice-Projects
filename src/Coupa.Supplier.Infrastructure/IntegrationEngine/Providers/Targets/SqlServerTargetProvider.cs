using System.Data;
using Coupa.Supplier.Domain.IntegrationEngine.Abstractions;
using Coupa.Supplier.Domain.IntegrationEngine.Models;
using Coupa.Supplier.Infrastructure.IntegrationEngine.Persistence;
using Microsoft.Data.SqlClient;

namespace Coupa.Supplier.Infrastructure.IntegrationEngine.Providers.Targets;

public sealed class SqlServerTargetProvider : ITargetProvider
{
    public string TargetType => "SqlServer";

    public async Task InsertSingleAsync(TargetConfiguration targetConfig, Dictionary<string, TableBatchData> mappedData, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(targetConfig.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        foreach (var (_, table) in mappedData)
        {
            for (var i = 0; i < table.RowCount; i++)
            {
                await using var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = DynamicSqlBuilder.BuildInsert(table.TableName, table.ColumnData.Keys.ToArray(), "@");

                foreach (var (column, values) in table.ColumnData)
                {
                    command.Parameters.AddWithValue($"@{column}", values[i] ?? DBNull.Value);
                }

                _ = await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }
    }

    public async Task InsertBulkAsync(TargetConfiguration targetConfig, Dictionary<string, TableBatchData> mappedData, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(targetConfig.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        foreach (var (_, table) in mappedData)
        {
            if (table.RowCount == 0)
            {
                continue;
            }

            var dt = new DataTable();
            foreach (var column in table.ColumnData.Keys)
            {
                dt.Columns.Add(column);
            }

            for (var rowIndex = 0; rowIndex < table.RowCount; rowIndex++)
            {
                var row = dt.NewRow();
                foreach (var (column, values) in table.ColumnData)
                {
                    row[column] = values[rowIndex] ?? DBNull.Value;
                }

                dt.Rows.Add(row);
            }

            using var bulkCopy = new SqlBulkCopy(connection) { DestinationTableName = table.TableName, BatchSize = table.RowCount };
            foreach (DataColumn column in dt.Columns)
            {
                bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
            }

            await bulkCopy.WriteToServerAsync(dt, cancellationToken);
        }
    }
}
