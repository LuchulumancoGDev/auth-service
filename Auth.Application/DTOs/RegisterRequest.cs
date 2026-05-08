using Auth.Domain.Enums;

namespace Auth.Application.DTOs;

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    public UserType UserType { get; set; }
    public string? TenantName { get; set; }
}