namespace Auth.Domain.Entities;

public abstract class TenantEntity : Entity
{
    public Guid TenantId { get; set; }
}
