using Coupa.Supplier.Domain.IntegrationEngine.Abstractions;

namespace Coupa.Supplier.Infrastructure.IntegrationEngine.Factories;

public sealed class TargetProviderFactory(IEnumerable<ITargetProvider> providers) : ITargetProviderFactory
{
    private readonly Dictionary<string, ITargetProvider> _providers = providers.ToDictionary(x => x.TargetType, StringComparer.OrdinalIgnoreCase);

    public ITargetProvider Resolve(string targetType)
    {
        if (_providers.TryGetValue(targetType, out var provider))
        {
            return provider;
        }

        throw new NotSupportedException($"Target provider '{targetType}' is not registered.");
    }
}
