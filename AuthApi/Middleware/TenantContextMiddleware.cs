using Auth.Infrastructure.Services;
using Microsoft.AspNetCore.Http;

namespace AuthApi.Middleware;

public class TenantContextMiddleware
{
    private readonly RequestDelegate _next;

    public TenantContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentTenantProvider tenantProvider)
    {
        try
        {
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var tenantId = tenantProvider.GetTenantId();
                context.Items["TenantId"] = tenantId;
            }
        }
        catch (InvalidOperationException)
        {
            // TenantId claim missing - will be handled by authorization
        }

        await _next(context);
    }
}
