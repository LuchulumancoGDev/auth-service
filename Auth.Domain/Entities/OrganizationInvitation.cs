namespace Auth.Domain.Entities;

/// <summary>
/// Represents a pending invitation to join an organization.
/// Invitations can be accepted by both new and existing users.
/// </summary>
public class OrganizationInvitation : Entity
{
    /// <summary>
    /// The organization (tenant) for which the invitation was created.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Navigation property to Tenant (Organization).
    /// </summary>
    public Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// The email address the invitation was sent to.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The role this invited user will receive upon acceptance.
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// Navigation property to Role.
    /// </summary>
    public Role Role { get; set; } = null!;

    /// <summary>
    /// Unique token for accepting this invitation.
    /// Should be cryptographically secure and unique.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Status of the invitation ("Pending", "Accepted", "Expired", "Revoked").
    /// </summary>
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// When this invitation expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// When the invitation was accepted (null if not yet accepted).
    /// </summary>
    public DateTime? AcceptedAt { get; set; }

    /// <summary>
    /// The user ID of the person who sent the invitation.
    /// Useful for audit trails and follow-ups.
    /// </summary>
    public string? InvitedBy { get; set; }

    /// <summary>
    /// The user ID of the person who accepted the invitation.
    /// </summary>
    public string? AcceptedBy { get; set; }
}
