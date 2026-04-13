namespace Auth.Domain.Entities;

public class UserPermission
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; }
}
