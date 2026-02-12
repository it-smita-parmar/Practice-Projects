using System.Text.Json.Nodes;
using Coupa.Supplier.Domain.IntegrationEngine.Models;

namespace Coupa.Supplier.Domain.IntegrationEngine.Abstractions;

public interface IMappingEngine
{
    Dictionary<string, TableBatchData> Transform(JsonNode sourceJson, MappingConfiguration mappingConfig);
}
