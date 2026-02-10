using CoupaInvoiceIngestion.Api.Application.Abstractions;
using CoupaInvoiceIngestion.Api.Domain.Entities;
using CoupaInvoiceIngestion.Api.Infrastructure.Configuration;
using CoupaInvoiceIngestion.Api.Infrastructure.Persistence.Oracle;
using Microsoft.Extensions.Options;

namespace CoupaInvoiceIngestion.Api.Infrastructure.Routing;

public sealed class InvoiceSinkRouter : IInvoiceSinkRouter
{
    private readonly TargetRoutingOptions _options;
    private readonly IServiceProvider _serviceProvider;

    public InvoiceSinkRouter(IOptions<TargetRoutingOptions> options, IServiceProvider serviceProvider)
    {
        _options = options.Value;
        _serviceProvider = serviceProvider;
    }

    public IReadOnlyCollection<IInvoiceSink> ResolveSinks(Invoice invoice)
    {
        var targets = _options.Targets
            .Where(t => (t.Regions.Count == 0 || t.Regions.Contains(invoice.Region.Value, StringComparer.OrdinalIgnoreCase))
                        && (t.Sources.Count == 0 || t.Sources.Contains(invoice.SourceSystem, StringComparer.OrdinalIgnoreCase)))
            .ToArray();

        return targets.Select(CreateSink).ToArray();
    }

    private IInvoiceSink CreateSink(TargetDefinition definition)
    {
        return definition.Type.ToLowerInvariant() switch
        {
            "oracle" => ActivatorUtilities.CreateInstance<OracleInvoiceSink>(_serviceProvider,
                definition.Name,
                definition.TableName,
                definition.ProfileName),
            "dynamics" => ActivatorUtilities.CreateInstance<DynamicsInvoiceSink>(_serviceProvider,
                definition.Name,
                definition.ProfileName),
            _ => throw new InvalidOperationException($"Unsupported sink type '{definition.Type}' for target '{definition.Name}'.")
        };
    }
}
