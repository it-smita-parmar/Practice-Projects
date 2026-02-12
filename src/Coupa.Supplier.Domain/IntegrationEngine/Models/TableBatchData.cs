namespace Coupa.Supplier.Domain.IntegrationEngine.Models;

public sealed class TableBatchData
{
    public string TableName { get; set; } = string.Empty;
    public Dictionary<string, List<object?>> ColumnData { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public int RowCount { get; set; }
}

public sealed class RecordError
{
    public string IntegrationName { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string RawPayload { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string StackTrace { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

public sealed record IntegrationRunResult(
    string IntegrationName,
    int TotalReceived,
    int TotalSucceeded,
    int TotalFailed,
    IReadOnlyCollection<RecordError> Errors);
