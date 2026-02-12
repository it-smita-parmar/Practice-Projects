namespace Coupa.Supplier.Domain.IntegrationEngine.Abstractions;

public interface ISourceProviderFactory
{
    ISourceProvider Resolve(string sourceType);
}
