using Auth.Domain.Entities;

namespace Auth.Infrastructure.Repositories;

public interface ITenantRepository : IRepository<Tenant>
{
    new Task<Tenant?> GetByIdAsync(Guid id);
    Task<Tenant?> GetByNameAsync(string name);
    Task<IEnumerable<Tenant>> GetActiveTenants();
}
