using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Coupa.Supplier.Api.Extensions;
using Coupa.Supplier.Application.Abstractions;
using Coupa.Supplier.Application.Configuration;
using Coupa.Supplier.Infrastructure.DependencyInjection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    var keyVaultUrl = builder.Configuration["KeyVault:Url"];
    if (!string.IsNullOrWhiteSpace(keyVaultUrl))
    {
        builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUrl), new DefaultAzureCredential(), new AzureKeyVaultConfigurationOptions());
    }
}

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks().AddDbContextCheck<Coupa.Supplier.Infrastructure.Persistence.SupplierDbContext>();
builder.Services.AddValidatorsFromAssemblyContaining<SupplierSyncRequestValidator>();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();
app.UseCustomMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

app.Run();
