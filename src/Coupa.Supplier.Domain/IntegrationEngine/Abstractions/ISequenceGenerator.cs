namespace Coupa.Supplier.Domain.IntegrationEngine.Abstractions;

public interface ISequenceGenerator
{
    long Next(string sequenceName);
}
