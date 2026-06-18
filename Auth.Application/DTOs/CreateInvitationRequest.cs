namespace Auth.Application.DTOs;

public class CreateInvitationRequest
{
    public string Email { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
}
