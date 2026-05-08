using Auth.Domain.Entities;

namespace Auth.Infrastructure.Repositories;

public interface IUserRepository : IRepository<ApplicationUser>
{
    Task<ApplicationUser?> GetByEmailAsync(string email);
    Task<ApplicationUser?> GetByIdWithTenantAsync(string userId);
    Task<IEnumerable<ApplicationUser>> GetByTenantIdAsync(Guid tenantId);
    Task<bool> EmailExistsAsync(string email);
}
