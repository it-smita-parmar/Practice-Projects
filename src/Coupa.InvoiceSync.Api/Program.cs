using Coupa.InvoiceSync.Api.Application.Abstractions;
using Coupa.InvoiceSync.Api.Application.Pipeline;
using Coupa.InvoiceSync.Api.Application.Services;
using Coupa.InvoiceSync.Api.Infrastructure.Coupa;
using Coupa.InvoiceSync.Api.Infrastructure.Mapping;
using Coupa.InvoiceSync.Api.Infrastructure.Options;
using Coupa.InvoiceSync.Api.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<CoupaApiOptions>(builder.Configuration.GetSection(CoupaApiOptions.SectionName));
builder.Services.Configure<MappingOptions>(builder.Configuration.GetSection(MappingOptions.SectionName));
builder.Services.Configure<RoutingOptions>(builder.Configuration.GetSection(RoutingOptions.SectionName));

builder.Services.AddHttpClient<ICoupaInvoiceClient, CoupaInvoiceClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<CoupaApiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
});
builder.Services.AddSingleton<IErrorStagingRepository, InMemoryErrorStagingRepository>();
builder.Services.AddSingleton<IInvoiceRepositoryFactory, InvoiceRepositoryFactory>();
builder.Services.AddSingleton<IInvoiceRepository, NorthAmericaSqlInvoiceRepository>();
builder.Services.AddSingleton<IInvoiceRepository, EuropeSqlInvoiceRepository>();
builder.Services.AddSingleton<IInvoiceRepository, IndiaSqlInvoiceRepository>();
builder.Services.AddSingleton<IInvoiceRepository, MsDynamicsInvoiceRepository>();

builder.Services.AddSingleton<IInvoiceProcessingMiddleware, GenericInvoiceMappingMiddleware>();
builder.Services.AddSingleton<IInvoiceProcessingMiddleware, InvoiceTypeValidationMiddleware>();
builder.Services.AddSingleton<IInvoiceProcessingPipeline, InvoiceProcessingPipeline>();
builder.Services.AddScoped<IInvoiceSyncService, InvoiceSyncService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
