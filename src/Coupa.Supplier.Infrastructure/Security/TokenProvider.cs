using Coupa.Supplier.Application.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Coupa.Supplier.Infrastructure.Security;

public sealed class TokenProvider(IConfiguration configuration, IOptions<CoupaOptions> coupaOptions) : ITokenProvider
{
    public Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        var token = coupaOptions.Value.BearerToken ?? configuration["Coupa:BearerToken"];
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("Coupa bearer token is not configured.");
        }

        return Task.FromResult(token);
    }
}
