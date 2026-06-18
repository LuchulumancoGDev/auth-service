namespace Auth.Infrastructure.Services;

public class MembershipInfo
{
    public Guid MembershipId { get; set; }
    public Guid TenantId { get; set; }
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
