using System.Data;
using Coupa.Supplier.Domain.IntegrationEngine.Abstractions;
using Coupa.Supplier.Domain.IntegrationEngine.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Coupa.Supplier.Infrastructure.IntegrationEngine.Configuration;

namespace Coupa.Supplier.Infrastructure.IntegrationEngine.Persistence;

public sealed class SqlServerErrorLogService(IOptions<IntegrationEngineOptions> options) : IErrorLogService
{
    public async Task LogAsync(RecordError error, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.Value.ErrorLogConnectionString))
        {
            return;
        }

        await using var connection = new SqlConnection(options.Value.ErrorLogConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.CommandText = """
                              INSERT INTO dbo.IntegrationErrorLog
                              (IntegrationName, TargetType, TableName, RawPayload, ErrorMessage, StackTrace, CreatedDate)
                              VALUES
                              (@IntegrationName, @TargetType, @TableName, @RawPayload, @ErrorMessage, @StackTrace, @CreatedDate)
                              """;

        command.Parameters.AddWithValue("@IntegrationName", error.IntegrationName);
        command.Parameters.AddWithValue("@TargetType", error.TargetType);
        command.Parameters.AddWithValue("@TableName", error.TableName);
        command.Parameters.AddWithValue("@RawPayload", error.RawPayload);
        command.Parameters.AddWithValue("@ErrorMessage", error.ErrorMessage);
        command.Parameters.AddWithValue("@StackTrace", error.StackTrace);
        command.Parameters.AddWithValue("@CreatedDate", error.CreatedDate);

        _ = await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
