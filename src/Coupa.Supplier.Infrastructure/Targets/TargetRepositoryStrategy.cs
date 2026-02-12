using Coupa.Supplier.Application.Interfaces;

namespace Coupa.Supplier.Infrastructure.Targets;

public sealed class TargetRepositoryStrategy(IEnumerable<ITargetSupplierRepository> repositories) : ITargetRepositoryStrategy
{
    public ITargetSupplierRepository Resolve(string? targetName)
    {
        var selected = targetName?.Trim().ToLowerInvariant() ?? "oracle";
        return repositories.FirstOrDefault(x => x.TargetName.Equals(selected, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Target repository '{selected}' is not registered.");
    }
}
