using Auth.Domain.Entities;
using Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Repositories;

public class OrganizationInvitationRepository : RepositoryBase<OrganizationInvitation>, IOrganizationInvitationRepository
{
    public OrganizationInvitationRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<OrganizationInvitation?> GetByTokenAsync(string token)
    {
        return await Context.OrganizationInvitations
            .FirstOrDefaultAsync(i => i.Token == token && i.Status == "Pending");
    }

    public async Task<IEnumerable<OrganizationInvitation>> GetPendingByTenantAsync(Guid tenantId)
    {
        return await Context.OrganizationInvitations
            .Where(i => i.TenantId == tenantId && i.Status == "Pending")
            .OrderBy(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task CreateInvitationAsync(OrganizationInvitation invitation)
    {
        await Context.OrganizationInvitations.AddAsync(invitation);
        await Context.SaveChangesAsync();
    }

    public async Task AcceptInvitationAsync(Guid invitationId, string acceptedByUserId)
    {
        var inv = await Context.OrganizationInvitations.FindAsync(invitationId);
        if (inv == null) return;
        inv.Status = "Accepted";
        inv.AcceptedAt = DateTime.UtcNow;
        inv.AcceptedBy = acceptedByUserId;
        await Context.SaveChangesAsync();
    }

    public async Task ExpireInvitationAsync(Guid invitationId)
    {
        var inv = await Context.OrganizationInvitations.FindAsync(invitationId);
        if (inv == null) return;
        inv.Status = "Expired";
        await Context.SaveChangesAsync();
    }
}
