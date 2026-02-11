namespace Coupa.InvoiceSync.Api.Domain.Entities;

/// <summary>
/// Represents the aggregate result returned after processing all invoices.
/// </summary>
public sealed class InvoiceSyncResult
{
    /// <summary>
    /// Gets or sets the number of invoices successfully persisted.
    /// </summary>
    public int Succeeded { get; set; }

    /// <summary>
    /// Gets or sets the number of invoices written to staging error storage.
    /// </summary>
    public int Failed { get; set; }

    /// <summary>
    /// Gets or sets the final status message presented in API responses.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
