using Microsoft.AspNetCore.Authorization;

namespace AuthApi.Authorization;

public class MembershipAuthorizationRequirement : IAuthorizationRequirement
{
    public string RequiredRoleName { get; }

    public MembershipAuthorizationRequirement(string requiredRoleName)
    {
        RequiredRoleName = requiredRoleName;
    }
}
