namespace Coupa.Supplier.Domain.IntegrationEngine.Models;

public sealed class IntegrationConfiguration
{
    public string IntegrationName { get; set; } = string.Empty;
    public string Direction { get; set; } = "Inbound";
    public SourceConfiguration Source { get; set; } = new();
    public TargetConfiguration Target { get; set; } = new();
    public string ProcessingMode { get; set; } = "Bulk";
    public int BatchSize { get; set; } = 1000;
    public int DegreeOfParallelism { get; set; } = 4;
    public MappingConfiguration Mapping { get; set; } = new();
}

public sealed class SourceConfiguration
{
    public string Type { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> Metadata { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class TargetConfiguration
{
    public string Type { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> Metadata { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class MappingConfiguration
{
    public List<TableMappingConfiguration> Tables { get; set; } = [];
}

public sealed class TableMappingConfiguration
{
    public string TableName { get; set; } = string.Empty;
    public string Alias { get; set; } = string.Empty;
    public string CollectionPath { get; set; } = "$";
    public string? PrimaryKey { get; set; }
    public string? GeneratePrimaryKey { get; set; }
    public string? ParentAlias { get; set; }
    public string? ParentKeyColumn { get; set; }
    public List<FieldMappingConfiguration> Fields { get; set; } = [];
}

public sealed class FieldMappingConfiguration
{
    public string SourcePath { get; set; } = string.Empty;
    public string TargetColumn { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public string DataType { get; set; } = "string";
    public int? MaxLength { get; set; }
}
