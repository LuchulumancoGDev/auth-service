using Auth.Domain.Entities;
using Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Repositories;

public class MembershipRepository : RepositoryBase<Membership>, IMembershipRepository
{
    public MembershipRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Membership?> GetMembershipByIdAsync(Guid id)
    {
        return await Context.Memberships
            .Include(m => m.User)
            .Include(m => m.Tenant)
            .Include(m => m.Role)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<Membership>> GetUserMembershipsAsync(string userId)
    {
        return await Context.Memberships
            .Include(m => m.Tenant)
            .Include(m => m.Role)
            .Where(m => m.UserId == userId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Membership>> GetUserActiveMembershipsAsync(string userId)
    {
        return await Context.Memberships
            .Include(m => m.Tenant)
            .Include(m => m.Role)
            .Where(m => m.UserId == userId && m.Status == "Active")
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Membership>> GetTenantMembersAsync(Guid tenantId)
    {
        return await Context.Memberships
            .Include(m => m.User)
            .Include(m => m.Role)
            .Where(m => m.TenantId == tenantId && m.Status == "Active")
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<Membership?> GetMembershipAsync(string userId, Guid tenantId)
    {
        return await Context.Memberships
            .Include(m => m.Role)
            .FirstOrDefaultAsync(m => m.UserId == userId && m.TenantId == tenantId);
    }

    public async Task CreateMembershipAsync(Membership membership)
    {
        await Context.Memberships.AddAsync(membership);
        await Context.SaveChangesAsync();
    }

    public async Task RemoveMembershipAsync(Guid membershipId)
    {
        var membership = await Context.Memberships.FindAsync(membershipId);
        if (membership != null)
        {
            Context.Memberships.Remove(membership);
            await Context.SaveChangesAsync();
        }
    }

    public async Task UpdateRoleAsync(Guid membershipId, Guid roleId)
    {
        var membership = await Context.Memberships.FindAsync(membershipId);
        if (membership != null)
        {
            membership.RoleId = roleId;
            membership.UpdatedAt = DateTime.UtcNow;
            Context.Memberships.Update(membership);
            await Context.SaveChangesAsync();
        }
    }

    public async Task<bool> IsOwnerAsync(string userId, Guid tenantId)
    {
        var tenant = await Context.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenantId && t.OwnerId == userId);
        return tenant != null;
    }

    public async Task<bool> IsMemberAsync(string userId, Guid tenantId)
    {
        return await Context.Memberships
            .AnyAsync(m => m.UserId == userId && m.TenantId == tenantId && m.Status == "Active");
    }

    public async Task<bool> HasRoleInTenantAsync(string userId, Guid tenantId, string roleName)
    {
        return await Context.Memberships
            .Include(m => m.Role)
            .AnyAsync(m => m.UserId == userId &&
                          m.TenantId == tenantId &&
                          m.Status == "Active" &&
                          m.Role.Name == roleName);
    }

    public async Task<Membership?> GetActiveMembershipWithRoleAsync(string userId, Guid tenantId)
    {
        return await Context.Memberships
            .Include(m => m.Role)
            .Include(m => m.Tenant)
            .FirstOrDefaultAsync(m => m.UserId == userId &&
                                      m.TenantId == tenantId &&
                                      m.Status == "Active");
    }
}
