using Auth.Domain.Entities;
using Auth.Domain.Enums;
using Auth.Infrastructure.Persistence;

namespace Auth.Infrastructure.Services;

public interface IDbInitializer
{
    Task InitializeAsync();
}

public class DbInitializer : IDbInitializer
{
    private readonly AppDbContext _context;

    public DbInitializer(AppDbContext context)
    {
        _context = context;
    }

    public async Task InitializeAsync()
    {
        // Seed system roles
        await SeedRolesAsync();
    }

    private async Task SeedRolesAsync()
    {
        // Check if roles already exist
        if (_context.Roles.Any())
        {
            return;
        }

        var roles = new List<Role>
        {
            new Role
            {
                Id = Guid.NewGuid(),
                Name = UserType.Admin.ToString(),
                Description = "Administrator with full system access",
                IsSystem = true,
                CreatedAt = DateTime.UtcNow
            },
            new Role
            {
                Id = Guid.NewGuid(),
                Name = UserType.Driver.ToString(),
                Description = "Driver role for transportation services",
                IsSystem = true,
                CreatedAt = DateTime.UtcNow
            },
            new Role
            {
                Id = Guid.NewGuid(),
                Name = UserType.Customer.ToString(),
                Description = "Customer role for end users",
                IsSystem = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        await _context.Roles.AddRangeAsync(roles);
        await _context.SaveChangesAsync();
    }
}
