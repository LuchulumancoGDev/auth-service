using Auth.Domain.Entities;
using Auth.Infrastructure.MultiTenancy;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly ITenantProvider _tenantProvider;
    public Guid? CurrentTenantId => _tenantProvider?.TenantId;

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<UserPermission> UserPermissions { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenantProvider) : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        //Tenant -> Users
        builder.Entity<ApplicationUser>()
            .HasOne(u => u.Tenant)
            .WithMany(t => t.Users)
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        //RefreshToken
        builder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        //RolePermission
        builder.Entity<RolePermission>()
            .HasOne(rp => rp.Role)
            .WithMany()
            .HasForeignKey(rp => rp.RoleId);

        builder.Entity<RolePermission>()
            .HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId);

        //UserPermission
        builder.Entity<UserPermission>()
            .HasOne(up => up.User)
            .WithMany(u => u.UserPermissions)
            .HasForeignKey(up => up.UserId);

        builder.Entity<UserPermission>()
            .HasOne(up => up.Permission)
            .WithMany(p => p.UserPermissions)
            .HasForeignKey(up => up.PermissionId);

        //Unique Permission Name
        builder.Entity<Permission>()
            .HasIndex(p => p.Name)
            .IsUnique();

        // Global query filters: allow all data when CurrentTenantId is null (e.g. migrations / admin ops),
        // otherwise filter to the tenant. Examples below for common tenant-scoped entities.

        // ApplicationUser has TenantId
        builder.Entity<ApplicationUser>().HasQueryFilter(u => !CurrentTenantId.HasValue || u.TenantId == CurrentTenantId);

        // RefreshToken -> uses User.TenantId
        builder.Entity<RefreshToken>().HasQueryFilter(rt => !CurrentTenantId.HasValue || rt.User.TenantId == CurrentTenantId);

        // If you add more tenant-scoped entities, apply filters similarly:
        // builder.Entity<YourEntity>().HasQueryFilter(e => !CurrentTenantId.HasValue || e.TenantId == CurrentTenantId);
    }
}