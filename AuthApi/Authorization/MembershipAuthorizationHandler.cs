using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace AuthApi.Authorization;

public class MembershipAuthorizationHandler : AuthorizationHandler<MembershipAuthorizationRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MembershipAuthorizationRequirement requirement)
    {
        if (!context.User.Identity?.IsAuthenticated == true)
        {
            return Task.CompletedTask;
        }

        var membershipsClaim = context.User.FindFirst("memberships")?.Value;
        if (string.IsNullOrEmpty(membershipsClaim))
        {
            return Task.CompletedTask;
        }

        Guid? tenantId = null;
        if (context.Resource is AuthorizationFilterContext authContext)
        {
            if (authContext.RouteData.Values.TryGetValue("tenantId", out var tenantValue) && tenantValue is string tenantString && Guid.TryParse(tenantString, out var parsedTenant))
            {
                tenantId = parsedTenant;
            }
            else if (authContext.RouteData.Values.TryGetValue("tenantId", out var tenantValueObj) && tenantValueObj is Guid tenantGuid)
            {
                tenantId = tenantGuid;
            }
        }

        try
        {
            var memberDtos = JsonSerializer.Deserialize<List<MembershipDto>>(membershipsClaim);
            if (memberDtos == null)
                return Task.CompletedTask;

            var matches = memberDtos.Where(m => m.RoleName == requirement.RequiredRoleName && m.Status == "Active");
            if (tenantId.HasValue)
            {
                matches = matches.Where(m => m.TenantId == tenantId.Value);
            }

            if (matches.Any())
            {
                context.Succeed(requirement);
            }
        }
        catch
        {
            // ignore malformed claim
        }

        return Task.CompletedTask;
    }

    private class MembershipDto
    {
        public Guid MembershipId { get; set; }
        public Guid TenantId { get; set; }
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
