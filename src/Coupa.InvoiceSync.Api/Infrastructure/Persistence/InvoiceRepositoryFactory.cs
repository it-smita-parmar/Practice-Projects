using Coupa.InvoiceSync.Api.Application.Abstractions;
using Coupa.InvoiceSync.Api.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Coupa.InvoiceSync.Api.Infrastructure.Persistence;

/// <summary>
/// Resolves destination repositories based on configurable route-to-repository mappings.
/// </summary>
public sealed class InvoiceRepositoryFactory(
    IEnumerable<IInvoiceRepository> repositories,
    IOptions<RoutingOptions> routingOptions) : IInvoiceRepositoryFactory
{
    private readonly Dictionary<string, IInvoiceRepository> _repositoryByName = repositories
        .ToDictionary(repository => repository.GetType().Name, StringComparer.OrdinalIgnoreCase);

    private readonly RoutingOptions _routingOptions = routingOptions.Value;

    /// <inheritdoc />
    public IInvoiceRepository Resolve(string region, string destinationSystem)
    {
        var routeKey = $"{region}:{destinationSystem}";
        if (!_routingOptions.RouteToRepository.TryGetValue(routeKey, out var repositoryName))
        {
            throw new KeyNotFoundException($"No route mapping exists for '{routeKey}'.");
        }

        if (_repositoryByName.TryGetValue(repositoryName, out var repository))
        {
            return repository;
        }

        throw new KeyNotFoundException($"Repository '{repositoryName}' was not registered for route '{routeKey}'.");
    }
}
