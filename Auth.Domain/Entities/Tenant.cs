using Auth.Domain.Enums;

namespace Auth.Domain.Entities;

public class Tenant : Entity
{
    public string Name { get; set; } = string.Empty;
    public TenantType Type { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
}