using Auth.Domain.Entities;
using Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Repositories;

public class TenantRepository : RepositoryBase<Tenant>, ITenantRepository
{
    public TenantRepository(AppDbContext context) : base(context)
    {
    }

    public override async Task<Tenant?> GetByIdAsync(Guid id)
    {
        return await DbSet
            .Include(t => t.Users)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Tenant?> GetByNameAsync(string name)
    {
        return await DbSet
            .FirstOrDefaultAsync(t => t.Name == name);
    }

    public async Task<IEnumerable<Tenant>> GetActiveTenants()
    {
        return await DbSet
            .Where(t => t.IsActive)
            .ToListAsync();
    }
}
