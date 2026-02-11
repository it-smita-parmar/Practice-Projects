namespace Coupa.InvoiceSync.Api.Domain.Enums;

/// <summary>
/// Defines the supported invoice categories received from Coupa.
/// </summary>
public enum InvoiceCategory
{
    /// <summary>
    /// Represents standard purchase invoices.
    /// </summary>
    Purchase,

    /// <summary>
    /// Represents licensing purchase invoices.
    /// </summary>
    Licensing,

    /// <summary>
    /// Represents resource billing invoices.
    /// </summary>
    ResourceBilling,

    /// <summary>
    /// Represents invoices that do not match a known category.
    /// </summary>
    Unknown
}
