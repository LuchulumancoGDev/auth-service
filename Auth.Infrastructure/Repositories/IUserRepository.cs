using Auth.Domain.Entities;

namespace Auth.Infrastructure.Repositories;

public interface IUserRepository : IRepository<ApplicationUser>
{
    Task<ApplicationUser?> GetByEmailAsync(string email);
    Task<ApplicationUser?> GetByIdAsync(string userId);
    Task<bool> EmailExistsAsync(string email);
}
