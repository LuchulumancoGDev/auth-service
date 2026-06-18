using Auth.Domain.Entities;

namespace Auth.Infrastructure.Repositories;

public interface IMembershipRepository : IRepository<Membership>
{
    /// <summary>
    /// Gets a specific membership by ID.
    /// </summary>
    Task<Membership?> GetMembershipByIdAsync(Guid id);

    /// <summary>
    /// Gets all memberships for a specific user.
    /// </summary>
    Task<IEnumerable<Membership>> GetUserMembershipsAsync(string userId);

    /// <summary>
    /// Gets all active memberships for a specific user.
    /// </summary>
    Task<IEnumerable<Membership>> GetUserActiveMembershipsAsync(string userId);

    /// <summary>
    /// Gets all members (memberships) of a specific organization/tenant.
    /// </summary>
    Task<IEnumerable<Membership>> GetTenantMembersAsync(Guid tenantId);

    /// <summary>
    /// Gets a specific membership for a user in a tenant.
    /// </summary>
    Task<Membership?> GetMembershipAsync(string userId, Guid tenantId);

    /// <summary>
    /// Creates a new membership.
    /// </summary>
    Task CreateMembershipAsync(Membership membership);

    /// <summary>
    /// Removes a membership (user leaves organization).
    /// </summary>
    Task RemoveMembershipAsync(Guid membershipId);

    /// <summary>
    /// Updates the role for a membership.
    /// </summary>
    Task UpdateRoleAsync(Guid membershipId, Guid roleId);

    /// <summary>
    /// Checks if a user is the owner of a tenant.
    /// </summary>
    Task<bool> IsOwnerAsync(string userId, Guid tenantId);

    /// <summary>
    /// Checks if a user is a member of a tenant.
    /// </summary>
    Task<bool> IsMemberAsync(string userId, Guid tenantId);

    /// <summary>
    /// Checks if a user has a specific role in a tenant.
    /// </summary>
    Task<bool> HasRoleInTenantAsync(string userId, Guid tenantId, string roleName);

    /// <summary>
    /// Gets the active membership with role details for a user in a tenant.
    /// </summary>
    Task<Membership?> GetActiveMembershipWithRoleAsync(string userId, Guid tenantId);
}
