using Coupa.Supplier.Application.Configuration;
using Coupa.Supplier.Application.Interfaces;
using Coupa.Supplier.Application.Services;
using Coupa.Supplier.Infrastructure.External.Coupa;
using Coupa.Supplier.Infrastructure.Mapping;
using Coupa.Supplier.Infrastructure.Persistence;
using Coupa.Supplier.Infrastructure.Security;
using Coupa.Supplier.Infrastructure.Targets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace Coupa.Supplier.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CoupaOptions>(configuration.GetSection(CoupaOptions.SectionName));
        services.Configure<SyncOptions>(configuration.GetSection(SyncOptions.SectionName));

        services.AddDbContext<SupplierDbContext>(options =>
            options.UseOracle(configuration.GetConnectionString("OraclePrimary"), oracle =>
                oracle.CommandTimeout(120)));

        services.AddScoped<IUnitOfWork, OracleUnitOfWork>();
        services.AddScoped<ITargetSupplierRepository, OracleTargetRepository>();
        services.AddScoped<ITargetSupplierRepository, SqlTargetRepository>();
        services.AddScoped<ITargetRepositoryStrategy, TargetRepositoryStrategy>();
        services.AddScoped<ISupplierSyncService, SupplierSyncService>();
        services.AddScoped<ITokenProvider, TokenProvider>();
        services.AddSingleton<IMappingConfigProvider, JsonMappingConfigProvider>();
        services.AddScoped<CoupaSupplierMapper>();

        services.AddHttpClient<ICoupaSupplierClient, CoupaSupplierClient>((provider, client) =>
            {
                var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<CoupaOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(120);
            })
            .AddPolicyHandler(GetRetryPolicy());

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        => HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));
}
