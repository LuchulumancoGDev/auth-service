namespace Auth.Application.DTOs;

public class UserDto
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public Guid TenantId { get; set; }
    public string UserType { get; set; }
    public bool IsEmailVerified { get; set; }
}
