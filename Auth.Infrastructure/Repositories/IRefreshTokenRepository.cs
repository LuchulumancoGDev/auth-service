using Auth.Domain.Entities;

namespace Auth.Infrastructure.Repositories;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, Guid tenantId);
    Task<IEnumerable<RefreshToken>> GetActiveTokensByUserAsync(string userId, Guid tenantId);
    Task<IEnumerable<RefreshToken>> GetRevokedTokensByUserAsync(string userId, Guid tenantId);
}
