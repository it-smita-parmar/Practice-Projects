using System.Text.Json.Nodes;
using Coupa.Supplier.Domain.IntegrationEngine.Models;

namespace Coupa.Supplier.Domain.IntegrationEngine.Abstractions;

public interface ISourceProvider
{
    string SourceType { get; }
    Task<JsonArray> GetDataAsync(SourceConfiguration sourceConfig, CancellationToken cancellationToken);
    Task SendDataAsync(SourceConfiguration sourceConfig, JsonObject payload, CancellationToken cancellationToken);
}
