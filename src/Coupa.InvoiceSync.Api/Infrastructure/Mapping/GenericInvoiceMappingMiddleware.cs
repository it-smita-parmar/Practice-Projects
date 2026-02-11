using System.Globalization;
using System.Reflection;
using Coupa.InvoiceSync.Api.Application.Pipeline;
using Coupa.InvoiceSync.Api.Domain.Entities;
using Coupa.InvoiceSync.Api.Domain.Enums;
using Coupa.InvoiceSync.Api.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Coupa.InvoiceSync.Api.Infrastructure.Mapping;

/// <summary>
/// Maps source Coupa payload fields into a canonical invoice model using configurable field maps.
/// </summary>
public sealed class GenericInvoiceMappingMiddleware(IOptions<MappingOptions> options) : IInvoiceProcessingMiddleware
{
    private readonly MappingOptions _mappingOptions = options.Value;

    /// <inheritdoc />
    public Task InvokeAsync(InvoiceProcessingContext context, Func<Task> next, CancellationToken cancellationToken)
    {
        var profile = _mappingOptions.Profiles.GetValueOrDefault("CoupaToCanonical")
            ?? throw new InvalidOperationException("Mapping profile 'CoupaToCanonical' is missing.");

        var canonical = new CanonicalInvoice();
        foreach (var fieldMap in profile.FieldMappings)
        {
            if (!context.SourceInvoice.Payload.TryGetProperty(fieldMap.SourceField, out var sourceValue))
            {
                continue;
            }

            SetTargetValue(canonical, fieldMap.TargetField, sourceValue);
        }

        canonical.InvoiceId = context.SourceInvoice.InvoiceId;
        canonical.Region = context.SourceInvoice.Region;
        canonical.DestinationSystem = context.SourceInvoice.DestinationSystem;
        canonical.Category = NormalizeCategory(context.SourceInvoice.InvoiceType);
        context.CanonicalInvoice = canonical;

        return next();
    }

    private static void SetTargetValue(CanonicalInvoice invoice, string targetField, System.Text.Json.JsonElement sourceValue)
    {
        var property = typeof(CanonicalInvoice).GetProperty(targetField, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
            ?? throw new InvalidOperationException($"Target field '{targetField}' does not exist on CanonicalInvoice.");

        object convertedValue = property.PropertyType.Name switch
        {
            nameof(String) => sourceValue.GetString() ?? string.Empty,
            nameof(Decimal) => sourceValue.GetDecimal(),
            nameof(DateTimeOffset) => DateTimeOffset.Parse(sourceValue.GetString() ?? DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture),
            _ => throw new InvalidOperationException($"Type '{property.PropertyType.Name}' is not supported by the mapping middleware.")
        };

        property.SetValue(invoice, convertedValue);
    }

    private static InvoiceCategory NormalizeCategory(string sourceType) => sourceType.ToLowerInvariant() switch
    {
        "purchase" => InvoiceCategory.Purchase,
        "licensing" => InvoiceCategory.Licensing,
        "resource" => InvoiceCategory.ResourceBilling,
        _ => InvoiceCategory.Unknown
    };
}
