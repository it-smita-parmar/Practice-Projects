using Coupa.InvoiceSync.Api.Application.Abstractions;

namespace Coupa.InvoiceSync.Api.Infrastructure.Persistence;

/// <summary>
/// Stores failed invoice payloads in an in-memory collection that represents a staging error table.
/// </summary>
public sealed class InMemoryErrorStagingRepository : IErrorStagingRepository
{
    private static readonly List<StagingErrorRecord> ErrorRows = [];

    /// <inheritdoc />
    public Task InsertErrorAsync(string invoiceId, string payload, string error, CancellationToken cancellationToken)
    {
        ErrorRows.Add(new StagingErrorRecord
        {
            InvoiceId = invoiceId,
            Payload = payload,
            Error = error,
            CreatedUtc = DateTimeOffset.UtcNow
        });

        return Task.CompletedTask;
    }

    /// <summary>
    /// Represents one staging error row.
    /// </summary>
    private sealed class StagingErrorRecord
    {
        /// <summary>
        /// Gets or sets invoice id.
        /// </summary>
        public string InvoiceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets failed payload JSON.
        /// </summary>
        public string Payload { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets exception message.
        /// </summary>
        public string Error { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets error creation timestamp.
        /// </summary>
        public DateTimeOffset CreatedUtc { get; set; }
    }
}
