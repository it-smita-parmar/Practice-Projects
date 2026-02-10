using CoupaInvoiceIngestion.Api.Domain.Enums;
using CoupaInvoiceIngestion.Api.Domain.ValueObjects;

namespace CoupaInvoiceIngestion.Api.Domain.Entities;

public sealed record Invoice(
    string InvoiceId,
    string SupplierName,
    decimal Amount,
    string Currency,
    DateTime InvoiceDate,
    InvoiceKind Kind,
    InvoiceRegion Region,
    string SourceSystem,
    IReadOnlyDictionary<string, object?> Attributes);
