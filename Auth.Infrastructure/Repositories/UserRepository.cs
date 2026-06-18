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
            .FirstOrDefaultAsync(u => u.NormalizedEmail == email.ToUpper());
    }

    public async Task<ApplicationUser?> GetByIdAsync(string userId)
    {
        return await DbSet
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await DbSet
            .AnyAsync(u => u.NormalizedEmail == email.ToUpper());
    }
}
