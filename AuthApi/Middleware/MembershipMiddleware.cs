using System.Text.Json;
using Auth.Infrastructure.Services;
using Microsoft.AspNetCore.Http;

namespace AuthApi.Middleware;

public class MembershipMiddleware
{
    private readonly RequestDelegate _next;

    public MembershipMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentMembershipProvider provider)
    {
        var user = context.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            var membershipsClaim = user.FindFirst("memberships")?.Value;
            if (!string.IsNullOrEmpty(membershipsClaim))
            {
                try
                {
                    var docs = JsonSerializer.Deserialize<List<MembershipDto>>(membershipsClaim);
                    if (docs != null)
                    {
                        var memberships = docs.Select(d => new MembershipInfo
                        {
                            MembershipId = d.MembershipId,
                            TenantId = d.TenantId,
                            RoleId = d.RoleId,
                            RoleName = d.RoleName,
                            Status = d.Status
                        }).ToList();

                        provider.Memberships = memberships;
                        provider.CurrentMembership = memberships.FirstOrDefault();
                    }
                }
                catch
                {
                    // ignore malformed claim
                }
            }
        }

        await _next(context);
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
