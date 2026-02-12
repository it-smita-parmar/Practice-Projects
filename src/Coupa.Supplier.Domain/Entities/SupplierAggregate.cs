using Coupa.Supplier.Domain.ValueObjects;

namespace Coupa.Supplier.Domain.Entities;

public sealed class SupplierAggregate
{
    public long SupplierId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Status { get; init; }
    public string Source { get; init; } = "COUPA";
    public string RawJson { get; init; } = string.Empty;
    public IReadOnlyCollection<SupplierSite> Sites { get; init; } = [];
    public IReadOnlyCollection<SupplierContact> Contacts { get; init; } = [];
    public IReadOnlyCollection<SupplierAddress> Addresses { get; init; } = [];
    public IReadOnlyCollection<RemitToAddress> RemitToAddresses { get; init; } = [];
}

public sealed class SupplierSite
{
    public long SiteId { get; init; }
    public string? Code { get; init; }
    public string? Name { get; init; }
    public string? PoMethod { get; init; }
    public string? PoChangeMethod { get; init; }
    public bool Active { get; init; }
}

public sealed class SupplierContact
{
    public long ContactId { get; init; }
    public string Kind { get; init; } = "PRIMARY";
}

public sealed class SupplierAddress
{
    public long? AddressId { get; init; }
    public string? LocationCode { get; init; }
    public string? StateIsoCode { get; init; }
    public bool Active { get; init; }
}

public sealed class RemitToAddress
{
    public long AddressId { get; init; }
    public string? RemitToCode { get; init; }
    public string? Name { get; init; }
    public string? Street1 { get; init; }
    public string? Street2 { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? PostalCode { get; init; }
    public bool Active { get; init; }
    public CoupaCountry? Country { get; init; }
}
