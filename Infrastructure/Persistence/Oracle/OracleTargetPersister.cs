using CoupaInvoiceIngestion.Api.Application.Abstractions;
using CoupaInvoiceIngestion.Api.Infrastructure.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace CoupaInvoiceIngestion.Api.Infrastructure.Persistence.Oracle;

public sealed class OracleTargetPersister(
    IOracleConnectionFactory connectionFactory,
    ILogger<OracleTargetPersister> logger) : ITargetPersister
{
    public string Type => "oracle";

    public async Task PersistAsync(TargetDefinition target, Dictionary<string, object?> payload, CancellationToken cancellationToken)
    {
        await using var connection = (OracleConnection)connectionFactory.Create(target.Name);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        var columns = string.Join(", ", payload.Keys);
        var parameterNames = string.Join(", ", payload.Keys.Select(k => $":{k}"));
        command.CommandText = $"INSERT INTO {target.TableName} ({columns}) VALUES ({parameterNames})";

        foreach (var item in payload)
        {
            command.Parameters.Add(new OracleParameter(item.Key, item.Value ?? DBNull.Value));
        }

        await command.ExecuteNonQueryAsync(cancellationToken);
        logger.LogInformation("Invoice persisted to Oracle target {TargetName}.", target.Name);
    }
}
