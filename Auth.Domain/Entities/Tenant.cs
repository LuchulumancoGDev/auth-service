using Auth.Domain.Enums;

namespace Auth.Domain.Entities;

public class Tenant : Entity
{
    /// <summary>
    /// The organization name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Unique slug for the organization (URL-safe identifier).
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// The user ID of the organization owner.
    /// Owner has all permissions and can transfer ownership.
    /// </summary>
    public string? OwnerId { get; set; }

    /// <summary>
    /// The subscription plan (e.g., "Free", "Pro", "Enterprise").
    /// </summary>
    public string Plan { get; set; } = "Free";

    /// <summary>
    /// The status of the organization ("Active", "Suspended", "Deleted").
    /// </summary>
    public string Status { get; set; } = "Active";

    /// <summary>
    /// Country where the organization is based.
    /// Useful for compliance and localization.
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// URL to the organization's logo.
    /// </summary>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// The type of account (Individual, Organization).
    /// </summary>
    public TenantType Type { get; set; }

    /// <summary>
    /// Whether the organization is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// All memberships for this organization.
    /// This is the new relationship model.
    /// </summary>
    public ICollection<Membership> Memberships { get; set; } = new List<Membership>();

    /// <summary>
    /// All pending invitations for this organization.
    /// </summary>
    public ICollection<OrganizationInvitation> Invitations { get; set; } = new List<OrganizationInvitation>();
}