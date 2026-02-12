using System.Text;

namespace Coupa.Supplier.Infrastructure.IntegrationEngine.Persistence;

public static class DynamicSqlBuilder
{
    public static string BuildInsert(string tableName, IReadOnlyCollection<string> columns, string parameterPrefix)
    {
        var list = columns.ToArray();
        var sql = new StringBuilder();
        sql.Append("INSERT INTO ").Append(tableName)
            .Append(" (").Append(string.Join(",", list)).Append(") VALUES (")
            .Append(string.Join(",", list.Select(c => $"{parameterPrefix}{c}")))
            .Append(')');

        return sql.ToString();
    }
}
