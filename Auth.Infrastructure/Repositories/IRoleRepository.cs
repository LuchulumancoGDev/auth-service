using Auth.Domain.Entities;

namespace Auth.Infrastructure.Repositories;

public interface IRoleRepository : IRepository<Role>
{
    Task<Role?> GetByNameAsync(string name);
    Task<IEnumerable<Role>> GetAllWithPermissionsAsync();
}
