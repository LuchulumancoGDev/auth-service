using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Auth.Infrastructure.MultiTenancy;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider)
    {
        var claim = context.User?.FindFirst("tenantId")?.Value;
        if (Guid.TryParse(claim, out var id))
            tenantProvider.TenantId = id;
        else
            tenantProvider.TenantId = null;

        await _next(context);
    }
}