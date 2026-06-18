using Auth.Domain.Entities;

namespace Auth.Infrastructure.Repositories;

public interface IOrganizationInvitationRepository : IRepository<OrganizationInvitation>
{
    Task<OrganizationInvitation?> GetByTokenAsync(string token);
    Task<IEnumerable<OrganizationInvitation>> GetPendingByTenantAsync(Guid tenantId);
    Task CreateInvitationAsync(OrganizationInvitation invitation);
    Task AcceptInvitationAsync(Guid invitationId, string acceptedByUserId);
    Task ExpireInvitationAsync(Guid invitationId);
}
