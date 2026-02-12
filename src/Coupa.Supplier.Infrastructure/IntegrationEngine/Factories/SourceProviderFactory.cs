using Coupa.Supplier.Domain.IntegrationEngine.Abstractions;

namespace Coupa.Supplier.Infrastructure.IntegrationEngine.Factories;

public sealed class SourceProviderFactory(IEnumerable<ISourceProvider> providers) : ISourceProviderFactory
{
    private readonly Dictionary<string, ISourceProvider> _providers = providers.ToDictionary(x => x.SourceType, StringComparer.OrdinalIgnoreCase);

    public ISourceProvider Resolve(string sourceType)
    {
        if (_providers.TryGetValue(sourceType, out var provider))
        {
            return provider;
        }

        throw new NotSupportedException($"Source provider '{sourceType}' is not registered.");
    }
}
