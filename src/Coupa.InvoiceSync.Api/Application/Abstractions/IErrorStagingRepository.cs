namespace Coupa.InvoiceSync.Api.Application.Abstractions;

/// <summary>
/// Stores failed invoice payloads in a staging error table.
/// </summary>
public interface IErrorStagingRepository
{
    /// <summary>
    /// Saves raw payload and error details for later reprocessing.
    /// </summary>
    /// <param name="invoiceId">The invoice identifier.</param>
    /// <param name="payload">The original JSON payload.</param>
    /// <param name="error">The processing error message.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    Task InsertErrorAsync(string invoiceId, string payload, string error, CancellationToken cancellationToken);
}
