using Auth.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace AuthApi.Authorization;

public static class AuthorizationPolicies
{
    public const string AdminOnlyPolicy = "AdminOnly";
    public const string DriverOnlyPolicy = "DriverOnly";
    public const string CustomerOnlyPolicy = "CustomerOnly";

    public static void AddAuthorizationPolicies(this AuthorizationOptions options)
    {
        // Admin-only policy
        options.AddPolicy(AdminOnlyPolicy, policy =>
            policy.RequireClaim("role", UserType.Admin.ToString()));

        // Driver-only policy
        options.AddPolicy(DriverOnlyPolicy, policy =>
            policy.RequireClaim("role", UserType.Driver.ToString()));

        // Customer-only policy
        options.AddPolicy(CustomerOnlyPolicy, policy =>
            policy.RequireClaim("role", UserType.Customer.ToString()));

        // Organization admin membership policy
        options.AddPolicy("OrganizationAdminOnly", policy =>
            policy.Requirements.Add(new MembershipAuthorizationRequirement("Admin")));

        // Authenticated user policy (any valid role)
        options.AddPolicy("Authenticated", policy =>
            policy.RequireAuthenticatedUser());
    }
}