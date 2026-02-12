namespace Coupa.Supplier.Infrastructure.Security;

public interface ITokenProvider
{
    Task<string> GetTokenAsync(CancellationToken cancellationToken);
}
