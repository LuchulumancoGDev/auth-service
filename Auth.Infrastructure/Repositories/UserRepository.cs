using Auth.Domain.Entities;
using Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Repositories;

public class UserRepository : RepositoryBase<ApplicationUser>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email)
    {
        return await DbSet
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.NormalizedEmail == email.ToUpper());
    }

    public async Task<ApplicationUser?> GetByIdWithTenantAsync(string userId)
    {
        return await DbSet
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<IEnumerable<ApplicationUser>> GetByTenantIdAsync(Guid tenantId)
    {
        return await DbSet
            .Where(u => u.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await DbSet
            .AnyAsync(u => u.NormalizedEmail == email.ToUpper());
    }
}
