using System.Net.Http.Headers;
using System.Net.Http.Json;
using CoupaInvoiceIngestion.Api.Application.Abstractions;
using CoupaInvoiceIngestion.Api.Contracts;
using CoupaInvoiceIngestion.Api.Domain.Entities;
using CoupaInvoiceIngestion.Api.Domain.Enums;
using CoupaInvoiceIngestion.Api.Domain.ValueObjects;
using CoupaInvoiceIngestion.Api.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace CoupaInvoiceIngestion.Api.Infrastructure.Clients;

public sealed class CoupaClient(HttpClient httpClient, IOptions<CoupaOptions> options) : ICoupaClient
{
    private readonly CoupaOptions _options = options.Value;

    public async Task<IReadOnlyCollection<Invoice>> GetInvoicesAsync(CancellationToken cancellationToken)
    {
        httpClient.BaseAddress = new Uri(_options.BaseUrl);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        var response = await httpClient.GetFromJsonAsync<CoupaInvoicesResponse>(_options.InvoicesPath, cancellationToken)
            ?? new CoupaInvoicesResponse();

        return response.Invoices.Select(ToDomain).ToArray();
    }

    private static Invoice ToDomain(CoupaInvoiceDto dto)
    {
        var kind = dto.Category?.Trim().ToLowerInvariant() switch
        {
            "licensing" => InvoiceKind.LicensingPurchase,
            "resource" => InvoiceKind.ResourceBilling,
            "thirdparty" => InvoiceKind.ThirdParty,
            _ => InvoiceKind.ActualInvoice
        };

        return new Invoice(
            dto.Id ?? Guid.NewGuid().ToString("N"),
            dto.SupplierName ?? "Unknown",
            dto.Total,
            dto.Currency ?? "USD",
            dto.InvoiceDate,
            kind,
            InvoiceRegion.From(dto.Region ?? "GLOBAL"),
            dto.SourceSystem ?? "COUPA",
            dto.Metadata);
    }
}
