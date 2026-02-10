using CoupaInvoiceIngestion.Api.Application.Abstractions;
using CoupaInvoiceIngestion.Api.Domain.Entities;
using Oracle.ManagedDataAccess.Client;

namespace CoupaInvoiceIngestion.Api.Infrastructure.Persistence.Oracle;

public sealed class OracleInvoiceSink(
    string name,
    string tableName,
    string profileName,
    IOracleConnectionFactory connectionFactory,
    IInvoiceMappingEngine mappingEngine,
    IInvoiceMappedPayloadStore mappedPayloadStore,
    ILogger<OracleInvoiceSink> logger) : IInvoiceSink
{
    public string Name => name;
    public string ProfileName => profileName;

    public async Task PersistAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        var mappedData = mappedPayloadStore.Get(invoice.InvoiceId, Name) ?? mappingEngine.Map(invoice, profileName);

        await using var connection = (OracleConnection)connectionFactory.Create(name);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        var columns = string.Join(", ", mappedData.Keys);
        var parameterNames = string.Join(", ", mappedData.Keys.Select(k => $":{k}"));
        command.CommandText = $"INSERT INTO {tableName} ({columns}) VALUES ({parameterNames})";

        foreach (var item in mappedData)
        {
            command.Parameters.Add(new OracleParameter(item.Key, item.Value ?? DBNull.Value));
        }

        await command.ExecuteNonQueryAsync(cancellationToken);
        logger.LogInformation("Invoice {InvoiceId} persisted to Oracle target {TargetName}.", invoice.InvoiceId, name);
    }
}
