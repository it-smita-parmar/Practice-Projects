using Coupa.Supplier.Application.IntegrationEngine;
using Coupa.Supplier.Domain.IntegrationEngine.Abstractions;
using Coupa.Supplier.Infrastructure.IntegrationEngine.Configuration;
using Coupa.Supplier.Infrastructure.IntegrationEngine.Factories;
using Coupa.Supplier.Infrastructure.IntegrationEngine.Mapping;
using Coupa.Supplier.Infrastructure.IntegrationEngine.Persistence;
using Coupa.Supplier.Infrastructure.IntegrationEngine.Providers.Sources;
using Coupa.Supplier.Infrastructure.IntegrationEngine.Providers.Targets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;

namespace Coupa.Supplier.Infrastructure.IntegrationEngine.DependencyInjection;

public static class IntegrationEngineServiceCollectionExtensions
{
    public static IServiceCollection AddIntegrationEngine(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<IntegrationEngineOptions>(configuration.GetSection(IntegrationEngineOptions.SectionName));

        services.AddHttpClient(nameof(CoupaSourceProvider), (provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<IntegrationEngineOptions>>().Value;
                if (!string.IsNullOrWhiteSpace(options.CoupaBaseUrl))
                {
                    client.BaseAddress = new Uri(options.CoupaBaseUrl);
                }
            })
            .AddPolicyHandler(GetRetryPolicy());

        services.AddHttpClient(nameof(CoupaTargetProvider), (provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<IntegrationEngineOptions>>().Value;
                if (!string.IsNullOrWhiteSpace(options.CoupaBaseUrl))
                {
                    client.BaseAddress = new Uri(options.CoupaBaseUrl);
                }
            })
            .AddPolicyHandler(GetRetryPolicy());

        services.AddHttpClient(nameof(MSDynamicsTargetProvider), (provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<IntegrationEngineOptions>>().Value;
                if (!string.IsNullOrWhiteSpace(options.MsDynamicsBaseUrl))
                {
                    client.BaseAddress = new Uri(options.MsDynamicsBaseUrl);
                }
            })
            .AddPolicyHandler(GetRetryPolicy());

        services.AddSingleton<IIntegrationConfigProvider, FileIntegrationConfigProvider>();
        services.AddSingleton<ISequenceGenerator, InMemorySequenceGenerator>();
        services.AddScoped<IErrorLogService, SqlServerErrorLogService>();
        services.AddSingleton<IMappingEngine, MetadataMappingEngine>();

        services.AddScoped<ISourceProvider, CoupaSourceProvider>();
        services.AddScoped<ISourceProvider, SqlSourceProvider>();
        services.AddScoped<ISourceProviderFactory, SourceProviderFactory>();

        services.AddScoped<ITargetProvider, OracleTargetProvider>();
        services.AddScoped<ITargetProvider, OracleUsTargetProvider>();
        services.AddScoped<ITargetProvider, SqlServerTargetProvider>();
        services.AddScoped<ITargetProvider, MSDynamicsTargetProvider>();
        services.AddScoped<ITargetProvider, CoupaTargetProvider>();
        services.AddScoped<ITargetProviderFactory, TargetProviderFactory>();

        services.AddScoped<IIntegrationOrchestrator, IntegrationOrchestrator>();

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        => HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromMilliseconds(250 * Math.Pow(2, attempt)));
}
