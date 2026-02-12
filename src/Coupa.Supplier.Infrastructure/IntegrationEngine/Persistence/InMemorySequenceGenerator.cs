using System.Collections.Concurrent;
using Coupa.Supplier.Domain.IntegrationEngine.Abstractions;

namespace Coupa.Supplier.Infrastructure.IntegrationEngine.Persistence;

public sealed class InMemorySequenceGenerator : ISequenceGenerator
{
    private readonly ConcurrentDictionary<string, long> _sequences = new(StringComparer.OrdinalIgnoreCase);

    public long Next(string sequenceName)
        => _sequences.AddOrUpdate(sequenceName, 1, (_, current) => current + 1);
}
