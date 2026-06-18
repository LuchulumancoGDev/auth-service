namespace Auth.Application.DTOs;

public class MembershipDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
}