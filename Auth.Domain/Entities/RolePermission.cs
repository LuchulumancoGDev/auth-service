using Microsoft.AspNetCore.Identity;

namespace Auth.Domain.Entities;

public class RolePermission
{
    public Guid Id { get; set; }
    public string RoleId { get; set; }
    public IdentityRole Role { get; set; }
    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; }
}
