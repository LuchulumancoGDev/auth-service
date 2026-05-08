namespace Auth.Domain.Entities;

public class Permission : TenantEntity
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public ICollection<RolePermission> RolePermissions { get; set; }
    public ICollection<UserPermission> UserPermissions { get; set; }
}
