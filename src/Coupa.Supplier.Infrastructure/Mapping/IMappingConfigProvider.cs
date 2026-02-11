namespace Coupa.Supplier.Infrastructure.Mapping;

public interface IMappingConfigProvider
{
    Task<MappingConfig> GetAsync(CancellationToken cancellationToken);
}
