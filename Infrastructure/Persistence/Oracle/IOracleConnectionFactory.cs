using System.Data;

namespace CoupaInvoiceIngestion.Api.Infrastructure.Persistence.Oracle;

public interface IOracleConnectionFactory
{
    IDbConnection Create(string targetName);
}
