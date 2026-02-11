using System.Text.Json;
using Coupa.Supplier.Domain.Entities;
using Coupa.Supplier.Domain.ValueObjects;

namespace Coupa.Supplier.Infrastructure.Mapping;

public sealed class CoupaSupplierMapper(IMappingConfigProvider mappingConfigProvider)
{
    public async Task<SupplierAggregate> MapAsync(JsonElement element, CancellationToken cancellationToken)
    {
        var mapping = await mappingConfigProvider.GetAsync(cancellationToken);
        var supplierTable = mapping.Tables.First(x => x.Alias == "SUPPLIERS");

        var supplier = new SupplierAggregate
        {
            SupplierId = GetLong(element, supplierTable.Fields.First(f => f.TargetColumn == "SUPPLIER_ID").SourcePath),
            Name = GetString(element, supplierTable.Fields.First(f => f.TargetColumn == "NAME").SourcePath) ?? string.Empty,
            Status = GetString(element, supplierTable.Fields.First(f => f.TargetColumn == "STATUS").SourcePath),
            Source = GetString(element, supplierTable.Fields.First(f => f.TargetColumn == "SOURCE").SourcePath) ?? "COUPA",
            RawJson = element.GetRawText(),
            Sites = MapSites(element),
            Contacts = MapContacts(element),
            Addresses = MapAddresses(element),
            RemitToAddresses = MapRemitTo(element)
        };

        return supplier;
    }

    private static IReadOnlyCollection<SupplierSite> MapSites(JsonElement supplier)
        => supplier.TryGetProperty("supplier-sites", out var sites)
            ? sites.EnumerateArray().Select(x => new SupplierSite
            {
                SiteId = GetLong(x, "$.id"),
                Code = GetString(x, "$.code"),
                Name = GetString(x, "$.name"),
                PoMethod = GetString(x, "$.po-method"),
                PoChangeMethod = GetString(x, "$.po-change-method"),
                Active = GetBool(x, "$.active")
            }).ToList()
            : [];

    private static IReadOnlyCollection<SupplierContact> MapContacts(JsonElement supplier)
        => supplier.TryGetProperty("contacts", out var contacts)
            ? contacts.EnumerateArray().Select(x => new SupplierContact
            {
                ContactId = GetLong(x, "$.id"),
                Kind = "PRIMARY"
            }).ToList()
            : [];

    private static IReadOnlyCollection<SupplierAddress> MapAddresses(JsonElement supplier)
        => supplier.TryGetProperty("supplier-addresses", out var addresses)
            ? addresses.EnumerateArray().Select(x => new SupplierAddress
            {
                AddressId = GetNullableLong(x, "$.id"),
                LocationCode = GetString(x, "$.location-code"),
                StateIsoCode = GetString(x, "$.state-iso-code"),
                Active = GetBool(x, "$.active")
            }).ToList()
            : [];

    private static IReadOnlyCollection<RemitToAddress> MapRemitTo(JsonElement supplier)
        => supplier.TryGetProperty("remit-to-addresses", out var addresses)
            ? addresses.EnumerateArray().Select(x => new RemitToAddress
            {
                AddressId = GetLong(x, "$.id"),
                RemitToCode = GetString(x, "$.remit-to-code"),
                Name = GetString(x, "$.name"),
                Street1 = GetString(x, "$.street1"),
                Street2 = GetString(x, "$.street2"),
                City = GetString(x, "$.city"),
                State = GetString(x, "$.state"),
                PostalCode = GetString(x, "$.postal-code"),
                Active = GetBool(x, "$.active"),
                Country = new CoupaCountry(GetString(x, "$.country.code"), GetString(x, "$.country.name"))
            }).ToList()
            : [];

    private static string? GetString(JsonElement element, string sourcePath)
    {
        if (sourcePath.StartsWith("constant("))
        {
            return sourcePath.Replace("constant('", string.Empty).Replace("')", string.Empty);
        }

        if (TryResolve(element, sourcePath, out var value))
        {
            return value.ValueKind == JsonValueKind.Null ? null : value.ToString();
        }

        return null;
    }

    private static long GetLong(JsonElement element, string sourcePath)
        => TryResolve(element, sourcePath, out var value) && value.TryGetInt64(out var number) ? number : 0;

    private static long? GetNullableLong(JsonElement element, string sourcePath)
        => TryResolve(element, sourcePath, out var value) && value.TryGetInt64(out var number) ? number : null;

    private static bool GetBool(JsonElement element, string sourcePath)
        => TryResolve(element, sourcePath, out var value) && value.ValueKind == JsonValueKind.True;

    private static bool TryResolve(JsonElement element, string sourcePath, out JsonElement value)
    {
        value = element;
        var segments = sourcePath.Trim().TrimStart('$', '.').Split('.', StringSplitOptions.RemoveEmptyEntries);
        foreach (var segment in segments)
        {
            if (!value.TryGetProperty(segment, out value))
            {
                return false;
            }
        }

        return true;
    }
}
