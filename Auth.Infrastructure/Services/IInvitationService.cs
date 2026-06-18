using Auth.Application.DTOs;

namespace Auth.Infrastructure.Services;

public interface IInvitationService
{
    Task<InvitationDto> CreateInvitationAsync(string requestingUserId, Guid tenantId, CreateInvitationRequest request);
    Task<InvitationDto?> GetInvitationByTokenAsync(string token);
    Task<IEnumerable<InvitationDto>> GetPendingInvitationsAsync(string requestingUserId, Guid tenantId);
    Task<bool> AcceptInvitationAsync(string token, string acceptingUserId);
}
