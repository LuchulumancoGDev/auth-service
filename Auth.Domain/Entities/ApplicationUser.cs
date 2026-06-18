using Auth.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Auth.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    public UserType UserType { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ExternalProvider { get; set; }
    public string? ExternalProviderId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
