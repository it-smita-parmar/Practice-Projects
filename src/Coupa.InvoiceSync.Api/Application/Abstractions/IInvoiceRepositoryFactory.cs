namespace Coupa.InvoiceSync.Api.Application.Abstractions;

/// <summary>
/// Selects a concrete target repository based on region and destination system.
/// </summary>
public interface IInvoiceRepositoryFactory
{
    /// <summary>
    /// Resolves the target repository using routing metadata.
    /// </summary>
    /// <param name="region">The source region.</param>
    /// <param name="destinationSystem">The destination system key.</param>
    /// <returns>A repository instance capable of persisting the invoice.</returns>
    IInvoiceRepository Resolve(string region, string destinationSystem);
}
