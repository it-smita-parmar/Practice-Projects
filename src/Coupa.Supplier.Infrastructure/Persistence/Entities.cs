using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Coupa.Supplier.Infrastructure.Persistence;

[Table("xxfin_coupa_suppliers", Schema = "xxfin")]
public sealed class CoupaSupplierRow
{
    [Key] [Column("STG_ID")] public decimal StgId { get; set; }
    [Column("SOURCE")] public string? Source { get; set; }
    [Column("SUPPLIER_ID")] public decimal SupplierId { get; set; }
    [Column("NAME")] public string? Name { get; set; }
    [Column("STATUS")] public string? Status { get; set; }
    [Column("CREATION_DATE")] public DateTime CreationDate { get; set; }
    [Column("CREATED_BY")] public string CreatedBy { get; set; } = "SYSTEM";
    [Column("LAST_UPDATE_DATE")] public DateTime LastUpdateDate { get; set; }
    [Column("LAST_UPDATED_BY")] public string LastUpdatedBy { get; set; } = "SYSTEM";
    [Column("INTEGRATION_STATUS")] public string IntegrationStatus { get; set; } = "NEW";
}

[Table("xxfin_coupa_error_logs", Schema = "xxfin")]
public sealed class CoupaErrorLogRow
{
    [Key] [Column("ID")] public decimal Id { get; set; }
    [Column("SOURCE")] public string Source { get; set; } = "COUPA";
    [Column("RAW_JSON")] public string RawJson { get; set; } = string.Empty;
    [Column("ERROR_MESSAGE")] public string ErrorMessage { get; set; } = string.Empty;
    [Column("CREATED_AT")] public DateTime CreatedAt { get; set; }
}
