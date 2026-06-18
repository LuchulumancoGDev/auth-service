using Auth.Domain.Enums;

namespace Auth.Application.DTOs;

public class RegisterRequest
{
    /// <summary>
    /// User's email address (used as username).
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's password (will be hashed).
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// User's full name.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is a business registration (creates organization).
    /// If true, an organization is created and user becomes owner.
    /// If false, user is created without organization.
    /// </summary>
    public bool IsBusinessRegistration { get; set; } = false;

    /// <summary>
    /// Organization name (required if IsBusinessRegistration is true).
    /// </summary>
    public string? OrganizationName { get; set; }

    /// <summary>
    /// Account type - Individual or Organization.
    /// </summary>
    public AccountType AccountType { get; set; }

    /// <summary>
    /// User type/role - Admin, Driver, Customer.
    /// </summary>
    public UserType UserType { get; set; }

    /// <summary>
    /// Legacy field: TenantName (deprecated in favor of OrganizationName).
    /// Kept for backward compatibility.
    /// </summary>
    [Obsolete("Use OrganizationName instead")]
    public string? TenantName { get; set; }
}