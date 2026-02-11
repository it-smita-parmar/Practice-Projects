using System.Net.Http.Headers;
using System.Text.Json;
using Coupa.Supplier.Application.Configuration;
using Coupa.Supplier.Application.Interfaces;
using Coupa.Supplier.Domain.Entities;
using Coupa.Supplier.Infrastructure.Mapping;
using Coupa.Supplier.Infrastructure.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Coupa.Supplier.Infrastructure.External.Coupa;

public sealed class CoupaSupplierClient(
    HttpClient httpClient,
    ITokenProvider tokenProvider,
    CoupaSupplierMapper mapper,
    ILogger<CoupaSupplierClient> logger,
    IOptions<CoupaOptions> coupaOptions) : ICoupaSupplierClient
{
    public async Task<IReadOnlyCollection<SupplierAggregate>> GetSuppliersAsync(CancellationToken cancellationToken)
    {
        var token = await tokenProvider.GetTokenAsync(cancellationToken);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await httpClient.GetAsync(coupaOptions.Value.SuppliersEndpoint, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var suppliers = new List<SupplierAggregate>();
        foreach (var element in document.RootElement.EnumerateArray())
        {
            suppliers.Add(await mapper.MapAsync(element, cancellationToken));
        }

        logger.LogInformation("Fetched {Count} suppliers from Coupa", suppliers.Count);
        return suppliers;
    }
}
