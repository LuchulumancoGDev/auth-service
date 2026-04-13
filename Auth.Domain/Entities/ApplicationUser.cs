using Microsoft.AspNetCore.Identity;

namespace Auth.Domain.Entities;

public class ApplicationUser: IdentityUser
{
    public string FullName { get; set; }
    public Guid? TenantId { get; set; }
    public Tenant Tenant { get; set; }
    public  string AccountType { get; set; }
    public string UserType { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; }
    public ICollection<UserPermission> UserPermissions { get; set; }
}
