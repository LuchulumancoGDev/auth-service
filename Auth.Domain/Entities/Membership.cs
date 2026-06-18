namespace Auth.Domain.Entities;

/// <summary>
/// Represents a user's participation in an organization with a specific role.
/// Separates identity (ApplicationUser) from organizational membership.
/// </summary>
public class Membership : Entity
{
    /// <summary>
    /// The user participating in the organization.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property to ApplicationUser.
    /// </summary>
    public ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// The organization (tenant) this membership belongs to.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Navigation property to Tenant (Organization).
    /// </summary>
    public Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// The role assigned to this user within the organization.
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// Navigation property to Role.
    /// </summary>
    public Role Role { get; set; } = null!;

    /// <summary>
    /// Status of this membership (e.g., "Active", "Revoked", "Pending").
    /// </summary>
    public string Status { get; set; } = "Active";

    /// <summary>
    /// When the user joined the organization.
    /// </summary>
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The user ID of the person who invited this member (if applicable).
    /// Null if user was created with business registration.
    /// </summary>
    public string? InvitedBy { get; set; }

    /// <summary>
    /// When this membership was last updated.
    /// Note: Explicitly overrides Entity.UpdatedAt for clarity in this context.
    /// </summary>
    public new DateTime? UpdatedAt { get; set; }
}
