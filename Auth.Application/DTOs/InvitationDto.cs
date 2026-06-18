namespace Auth.Application.DTOs;

public class InvitationDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? InvitedBy { get; set; }
    public string? AcceptedBy { get; set; }
    // Token is returned only on creation to allow sending invite links/emails
    public string? Token { get; set; }
}
