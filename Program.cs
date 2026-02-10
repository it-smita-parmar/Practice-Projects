using CoupaInvoiceIngestion.Api.Api;
using CoupaInvoiceIngestion.Api.Application.Abstractions;
using CoupaInvoiceIngestion.Api.Application.Services;
using CoupaInvoiceIngestion.Api.Infrastructure.Clients;
using CoupaInvoiceIngestion.Api.Infrastructure.Configuration;
using CoupaInvoiceIngestion.Api.Infrastructure.Mapping;
using CoupaInvoiceIngestion.Api.Infrastructure.Persistence.Oracle;
using CoupaInvoiceIngestion.Api.Infrastructure.Routing;
using CoupaInvoiceIngestion.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<CoupaOptions>(builder.Configuration.GetSection(CoupaOptions.SectionName));
builder.Services.Configure<InvoiceMappingOptions>(builder.Configuration.GetSection(InvoiceMappingOptions.SectionName));
builder.Services.Configure<TargetRoutingOptions>(builder.Configuration.GetSection(TargetRoutingOptions.SectionName));
builder.Services.Configure<OracleTargetsOptions>(builder.Configuration.GetSection(OracleTargetsOptions.SectionName));

builder.Services.AddHttpClient<ICoupaClient, CoupaClient>();
builder.Services.AddSingleton<IOracleConnectionFactory, OracleConnectionFactory>();
builder.Services.AddScoped<IInvoiceSyncService, InvoiceSyncService>();
builder.Services.AddScoped<IInvoiceSinkRouter, InvoiceSinkRouter>();
builder.Services.AddSingleton<IInvoiceMappingEngine, InvoiceMappingEngine>();
builder.Services.AddScoped<IInvoiceMappedPayloadStore, InvoiceMappedPayloadStore>();
builder.Services.AddScoped<IInvoiceProcessingMiddleware, InvoiceMappingMiddleware>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapInvoiceEndpoints();

app.Run();
