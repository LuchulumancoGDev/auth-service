using Auth.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace AuthApi.Authorization;

public static class AuthorizationPolicies
{
    public const string AdminOnlyPolicy = "AdminOnly";
    public const string DriverOnlyPolicy = "DriverOnly";
    public const string CustomerOnlyPolicy = "CustomerOnly";
    public const string ManagerOnlyPolicy = "ManagerOnly";
    public const string OwnerOnlyPolicy = "OwnerOnly";

    public static void AddAuthorizationPolicies(this AuthorizationOptions options)
    {
        // Organization role-based policies (using "role" claim from JWT)
        options.AddPolicy(AdminOnlyPolicy, policy =>
            policy.RequireClaim("role", UserType.Admin.ToString(), UserType.Owner.ToString()));

        options.AddPolicy(OwnerOnlyPolicy, policy =>
            policy.RequireClaim("role", UserType.Owner.ToString()));

        options.AddPolicy(ManagerOnlyPolicy, policy =>
            policy.RequireClaim("role", UserType.Manager.ToString(), UserType.Admin.ToString(), UserType.Owner.ToString()));

        options.AddPolicy(DriverOnlyPolicy, policy =>
            policy.RequireClaim("role", UserType.Driver.ToString()));

        options.AddPolicy(CustomerOnlyPolicy, policy =>
            policy.RequireClaim("role", UserType.Customer.ToString()));

        // Organization admin membership policy (checks membership claim for role)
        options.AddPolicy("OrganizationAdminOnly", policy =>
            policy.Requirements.Add(new MembershipAuthorizationRequirement("Admin")));

        // Organization owner membership policy
        options.AddPolicy("OrganizationOwnerOnly", policy =>
            policy.Requirements.Add(new MembershipAuthorizationRequirement("Owner")));

        // Authenticated user policy (any valid role)
        options.AddPolicy("Authenticated", policy =>
            policy.RequireAuthenticatedUser());
    }
}
