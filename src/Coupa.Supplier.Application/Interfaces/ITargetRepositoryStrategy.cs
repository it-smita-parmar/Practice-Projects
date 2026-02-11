namespace Coupa.Supplier.Application.Interfaces;

public interface ITargetRepositoryStrategy
{
    ITargetSupplierRepository Resolve(string? targetName);
}
