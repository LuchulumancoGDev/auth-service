using Auth.Domain.Entities;
using Auth.Domain.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Tenant> Tenants { get; set; }
    public new DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ===== TENANT CONFIGURATION =====
        builder.Entity<Tenant>()
            .HasKey(t => t.Id);

        builder.Entity<Tenant>()
            .Property(t => t.Name)
            .IsRequired();

        builder.Entity<Tenant>()
            .Property(t => t.Type)
            .HasConversion<int>();

        // ===== APPLICATION USER CONFIGURATION =====
        builder.Entity<ApplicationUser>()
            .Property(u => u.TenantId)
            .IsRequired();

        builder.Entity<ApplicationUser>()
            .HasOne(u => u.Tenant)
            .WithMany(t => t.Users)
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ApplicationUser>()
            .HasOne(u => u.Role)
            .WithMany()
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ApplicationUser>()
            .Property(u => u.AccountType)
            .HasConversion<int>();

        builder.Entity<ApplicationUser>()
            .Property(u => u.UserType)
            .HasConversion<int>();

        builder.Entity<ApplicationUser>()
            .HasIndex(u => new { u.TenantId, u.Email })
            .IsUnique()
            .HasFilter("[NormalizedEmail] IS NOT NULL");

        // Global query filter for ApplicationUser tenant isolation
        builder.Entity<ApplicationUser>()
            .HasQueryFilter(u => u.TenantId != Guid.Empty);

        // ===== ROLE CONFIGURATION =====
        builder.Entity<Role>()
            .HasKey(r => r.Id);

        builder.Entity<Role>()
            .Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Entity<Role>()
            .HasIndex(r => r.Name)
            .IsUnique();

        // ===== PERMISSION CONFIGURATION =====
        builder.Entity<Permission>()
            .HasKey(p => p.Id);

        builder.Entity<Permission>()
            .Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Entity<Permission>()
            .HasIndex(p => new { p.TenantId, p.Name })
            .IsUnique();

        // Global query filter for Permission tenant isolation
        builder.Entity<Permission>()
            .HasQueryFilter(p => p.TenantId != Guid.Empty);

        // ===== ROLE-PERMISSION CONFIGURATION =====
        builder.Entity<RolePermission>()
            .HasKey(rp => rp.Id);

        builder.Entity<RolePermission>()
            .HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RolePermission>()
            .HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RolePermission>()
            .HasIndex(rp => new { rp.RoleId, rp.PermissionId })
            .IsUnique();

        // ===== REFRESH TOKEN CONFIGURATION =====
        builder.Entity<RefreshToken>()
            .HasKey(rt => rt.Id);

        builder.Entity<RefreshToken>()
            .Property(rt => rt.TokenHash)
            .IsRequired();

        builder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RefreshToken>()
            .HasIndex(rt => new { rt.TenantId, rt.UserId })
            .HasFilter("[IsRevoked] = 0");

        // Global query filter for RefreshToken tenant isolation
        builder.Entity<RefreshToken>()
            .HasQueryFilter(rt => rt.TenantId != Guid.Empty);

        // Seed system roles
        SeedSystemRoles(builder);
    }

    private static void SeedSystemRoles(ModelBuilder builder)
    {
        var adminRole = new Role
        {
            Id = Guid.Parse("1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d"),
            Name = UserType.Admin.ToString(),
            Description = "Administrator with full system access",
            IsSystem = true,
            CreatedAt = DateTime.UtcNow
        };

        var driverRole = new Role
        {
            Id = Guid.Parse("2b3c4d5e-6f7a-8b9c-0d1e-2f3a4b5c6d7e"),
            Name = UserType.Driver.ToString(),
            Description = "Driver role for transportation services",
            IsSystem = true,
            CreatedAt = DateTime.UtcNow
        };

        var customerRole = new Role
        {
            Id = Guid.Parse("3c4d5e6f-7a8b-9c0d-1e2f-3a4b5c6d7e8f"),
            Name = UserType.Customer.ToString(),
            Description = "Customer role for end users",
            IsSystem = true,
            CreatedAt = DateTime.UtcNow
        };

        builder.Entity<Role>().HasData(adminRole, driverRole, customerRole);
    }
}