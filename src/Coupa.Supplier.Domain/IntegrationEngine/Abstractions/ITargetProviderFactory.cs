namespace Coupa.Supplier.Domain.IntegrationEngine.Abstractions;

public interface ITargetProviderFactory
{
    ITargetProvider Resolve(string targetType);
}
