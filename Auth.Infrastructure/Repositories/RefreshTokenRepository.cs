using Auth.Domain.Entities;
using Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Repositories;

public class RefreshTokenRepository : RepositoryBase<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, Guid tenantId)
    {
        return await DbSet
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash && rt.TenantId == tenantId && !rt.IsRevoked);
    }

    public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserAsync(string userId, Guid tenantId)
    {
        return await DbSet
            .Where(rt => rt.UserId == userId && rt.TenantId == tenantId && !rt.IsRevoked)
            .ToListAsync();
    }

    public async Task<IEnumerable<RefreshToken>> GetRevokedTokensByUserAsync(string userId, Guid tenantId)
    {
        return await DbSet
            .Where(rt => rt.UserId == userId && rt.TenantId == tenantId && rt.IsRevoked)
            .ToListAsync();
    }
}
