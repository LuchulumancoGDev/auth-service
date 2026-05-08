namespace Auth.Domain.Entities;

public abstract class AuditableEntity : Entity
{
    public new DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public new DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}