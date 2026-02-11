using System.Text.Json;
using Coupa.InvoiceSync.Api.Application.Abstractions;
using Coupa.InvoiceSync.Api.Contracts;
using Coupa.InvoiceSync.Api.Domain.Entities;
using Coupa.InvoiceSync.Api.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Coupa.InvoiceSync.Api.Infrastructure.Coupa;

/// <summary>
/// Calls Coupa REST endpoints and transforms responses into domain entities.
/// </summary>
public sealed class CoupaInvoiceClient(HttpClient httpClient, IOptions<CoupaApiOptions> options) : ICoupaInvoiceClient
{
    private readonly CoupaApiOptions _options = options.Value;

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<CoupaInvoice>> GetInvoicesAsync(CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, _options.GetInvoicesPath);
        request.Headers.Add("X-COUPA-API-KEY", _options.ApiKey);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<CoupaInvoicesResponse>(stream, cancellationToken: cancellationToken)
            ?? new CoupaInvoicesResponse();

        return payload.Invoices
            .Select(invoice => new CoupaInvoice
            {
                InvoiceId = invoice.Id,
                Region = invoice.Region,
                InvoiceType = invoice.InvoiceType,
                DestinationSystem = invoice.DestinationSystem,
                Payload = invoice.Payload
            })
            .ToArray();
    }
}
