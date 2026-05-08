namespace Auth.Domain.Entities;

public class Role : Entity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsSystem { get; set; } = true;
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
