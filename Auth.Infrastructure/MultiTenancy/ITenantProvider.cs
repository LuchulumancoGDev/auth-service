namespace Auth.Infrastructure.MultiTenancy;

public interface ITenantProvider
{
    Guid? TenantId { get; set; }
}