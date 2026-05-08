using Auth.Domain.Entities;

namespace Auth.Infrastructure.Repositories;

public interface IPermissionRepository : IRepository<Permission>
{
    Task<Permission?> GetByNameAsync(string name, Guid tenantId);
    Task<IEnumerable<Permission>> GetByTenantAsync(Guid tenantId);
}
