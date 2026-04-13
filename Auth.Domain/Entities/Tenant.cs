namespace Auth.Domain.Entities;

public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool IsOrganisation { get; set; }
    public ICollection<ApplicationUser> Users { get; set; }
}
