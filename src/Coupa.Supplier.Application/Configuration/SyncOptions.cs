namespace Coupa.Supplier.Application.Configuration;

public sealed class SyncOptions
{
    public const string SectionName = "SupplierSync";
    public int BatchSize { get; set; } = 1000;
    public int DegreeOfParallelism { get; set; } = 4;
    public string MappingFilePath { get; set; } = "config/coupa-mapping.json";
    public bool UseBulkInsertDefault { get; set; } = true;
}
