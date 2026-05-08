using Auth.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Auth.Infrastructure.Services;

public interface ICurrentTenantProvider
{
    Guid GetTenantId();
    string? GetUserId();
    ApplicationUser? GetCurrentUser();
}

public class CurrentTenantProvider : ICurrentTenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetTenantId()
    {
        var tenantClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("tenantId");

        if (tenantClaim == null || !Guid.TryParse(tenantClaim.Value, out var tenantId))
        {
            throw new InvalidOperationException("TenantId not found in JWT claims or invalid format.");
        }

        return tenantId;
    }

    public string? GetUserId()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public ApplicationUser? GetCurrentUser()
    {
        var userId = GetUserId();
        return string.IsNullOrEmpty(userId) ? null : new ApplicationUser { Id = userId };
    }
}
