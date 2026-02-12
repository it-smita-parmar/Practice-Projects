namespace Coupa.Supplier.Infrastructure.Mapping;

public sealed class MappingConfig
{
    public string Entity { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public int BatchSize { get; set; } = 1000;
    public List<TableMapping> Tables { get; set; } = [];
}

public sealed class TableMapping
{
    public string TableName { get; set; } = string.Empty;
    public string Alias { get; set; } = string.Empty;
    public string CollectionPath { get; set; } = string.Empty;
    public List<FieldMapping> Fields { get; set; } = [];
}

public sealed class FieldMapping
{
    public string SourcePath { get; set; } = string.Empty;
    public string TargetColumn { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public string DataType { get; set; } = string.Empty;
    public int? MaxLength { get; set; }
}
