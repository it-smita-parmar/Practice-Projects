using Coupa.Supplier.Api.Middleware;

namespace Coupa.Supplier.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseCustomMiddleware(this WebApplication app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<GlobalExceptionMiddleware>();
        return app;
    }
}
