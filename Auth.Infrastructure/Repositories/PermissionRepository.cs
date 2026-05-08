using Auth.Domain.Entities;
using Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Repositories;

public class PermissionRepository : RepositoryBase<Permission>, IPermissionRepository
{
    public PermissionRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Permission?> GetByNameAsync(string name, Guid tenantId)
    {
        return await DbSet
            .FirstOrDefaultAsync(p => p.Name == name && p.TenantId == tenantId);
    }

    public async Task<IEnumerable<Permission>> GetByTenantAsync(Guid tenantId)
    {
        return await DbSet
            .Where(p => p.TenantId == tenantId)
            .ToListAsync();
    }
}
