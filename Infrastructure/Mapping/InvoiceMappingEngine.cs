using System.Reflection;
using CoupaInvoiceIngestion.Api.Application.Abstractions;
using CoupaInvoiceIngestion.Api.Domain.Entities;
using CoupaInvoiceIngestion.Api.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace CoupaInvoiceIngestion.Api.Infrastructure.Mapping;

public sealed class InvoiceMappingEngine(IOptions<InvoiceMappingOptions> options) : IInvoiceMappingEngine
{
    private readonly InvoiceMappingOptions _options = options.Value;

    public Dictionary<string, object?> Map(Invoice invoice, string profileName)
    {
        if (!_options.Profiles.TryGetValue(profileName, out var profile))
        {
            throw new InvalidOperationException($"Mapping profile '{profileName}' is not configured.");
        }

        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var mapping in profile)
        {
            result[mapping.Key] = ResolveValue(invoice, mapping.Value);
        }

        return result;
    }

    private static object? ResolveValue(Invoice invoice, string sourcePath)
    {
        if (sourcePath.StartsWith("attributes.", StringComparison.OrdinalIgnoreCase))
        {
            var attributeKey = sourcePath["attributes.".Length..];
            return invoice.Attributes.TryGetValue(attributeKey, out var value) ? value : null;
        }

        var property = typeof(Invoice)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(p => p.Name.Equals(sourcePath, StringComparison.OrdinalIgnoreCase));

        return property?.GetValue(invoice);
    }
}
