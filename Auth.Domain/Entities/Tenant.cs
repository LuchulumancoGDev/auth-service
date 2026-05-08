namespace Auth.Domain.Entities;

public class Tenant : Entity
{
    public string Name { get; set; }
    public bool IsOrganisation { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<ApplicationUser> Users { get; set; }
}
