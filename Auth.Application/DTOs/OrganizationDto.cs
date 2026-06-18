namespace Auth.Application.DTOs;

public class OrganizationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Plan { get; set; }
    public string? Status { get; set; }
    public string? Country { get; set; }
    public string? LogoUrl { get; set; }
    public string Type { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    // Membership-specific fields (only populated when returning user's organizations)
    public Guid? MembershipId { get; set; }
    public string? Role { get; set; }
    public bool IsOwner { get; set; }
}