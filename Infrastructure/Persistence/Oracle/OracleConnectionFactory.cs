using System.Data;
using CoupaInvoiceIngestion.Api.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;

namespace CoupaInvoiceIngestion.Api.Infrastructure.Persistence.Oracle;

public sealed class OracleConnectionFactory(IOptions<OracleTargetsOptions> options) : IOracleConnectionFactory
{
    private readonly OracleTargetsOptions _options = options.Value;

    public IDbConnection Create(string targetName)
    {
        if (!_options.Connections.TryGetValue(targetName, out var connectionString))
        {
            throw new InvalidOperationException($"No Oracle connection string configured for target '{targetName}'.");
        }

        return new OracleConnection(connectionString);
    }
}
