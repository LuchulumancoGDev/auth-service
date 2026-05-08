namespace Auth.Application.DTOs;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string UserType { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
}