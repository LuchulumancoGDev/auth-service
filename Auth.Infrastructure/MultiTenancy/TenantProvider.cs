using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Auth.Infrastructure.MultiTenancy;

public class TenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public Guid? TenantId { get; set; }

    public TenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        // optional: try populate immediately when constructed (middleware will set on each request reliably)
        var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("tenantId")?.Value;
        if (Guid.TryParse(claim, out var id))
            TenantId = id;
    }
}