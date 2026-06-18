using Auth.Domain.Entities;

namespace Auth.Infrastructure.Services;

public interface IOrganizationService
{
    /// <summary>
    /// Creates a new organization (tenant) with the specified owner and role assignment.
    /// </summary>
    Task<Tenant> CreateOrganizationAsync(
        string name,
        string slug,
        string ownerId,
        string tenantType = "Organization");

    /// <summary>
    /// Creates a membership for a user in an organization with the specified role.
    /// </summary>
    Task<Membership> CreateMembershipAsync(
        string userId,
        Guid tenantId,
        Guid roleId,
        string? invitedBy = null);

    /// <summary>
    /// Gets an organization by ID.
    /// </summary>
    Task<Tenant?> GetOrganizationAsync(Guid id);

    /// <summary>
    /// Gets an organization by slug.
    /// </summary>
    Task<Tenant?> GetOrganizationBySlugAsync(string slug);

    /// <summary>
    /// Checks if a slug is unique across all organizations.
    /// </summary>
    Task<bool> IsSlugUniqueAsync(string slug);

    /// <summary>
    /// Generates a unique slug from an organization name.
    /// </summary>
    Task<string> GenerateUniqueSlugAsync(string organizationName);

    /// <summary>
    /// Gets all organizations owned by a user.
    /// </summary>
    Task<IEnumerable<Tenant>> GetUserOwnedOrganizationsAsync(string userId);
}
