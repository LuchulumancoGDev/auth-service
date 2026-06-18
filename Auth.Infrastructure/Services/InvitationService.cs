using Auth.Application.DTOs;
using Auth.Domain.Entities;
using Auth.Infrastructure.Repositories;
using System.Security.Cryptography;

namespace Auth.Infrastructure.Services;

public class InvitationService : IInvitationService
{
    private readonly IOrganizationInvitationRepository _invitationRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailSender _emailSender;

    public InvitationService(
        IOrganizationInvitationRepository invitationRepository,
        IMembershipRepository membershipRepository,
        IRoleRepository roleRepository,
        IUserRepository userRepository,
        IEmailSender emailSender)
    {
        _invitationRepository = invitationRepository;
        _membershipRepository = membershipRepository;
        _roleRepository = roleRepository;
        _userRepository = userRepository;
        _emailSender = emailSender;
    }

    public async Task<InvitationDto> CreateInvitationAsync(string requestingUserId, Guid tenantId, CreateInvitationRequest request)
    {
        // Validate role exists
        var role = await _roleRepository.GetByNameAsync(request.RoleName);
        if (role == null)
            throw new InvalidOperationException("Role not found");

        // Create secure token
        var token = GenerateSecureToken();

        var invitation = new OrganizationInvitation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = request.Email,
            RoleId = role.Id,
            Token = token,
            Status = "Pending",
            ExpiresAt = request.ExpiresAt ?? DateTime.UtcNow.AddDays(7),
            InvitedBy = requestingUserId,
            CreatedAt = DateTime.UtcNow
        };

        await _invitationRepository.CreateInvitationAsync(invitation);

        try
        {
            // fire-and-forget style: if email fails we don't rollback invitation
            await _emailSender.SendInvitationAsync(invitation.Email, invitation.Token, invitation.TenantId, invitation.InvitedBy);
        }
        catch
        {
            // swallowing exceptions here; consider logging in real implementation
        }

        return new InvitationDto
        {
            Id = invitation.Id,
            TenantId = tenantId,
            Email = invitation.Email,
            RoleName = role.Name,
            Status = invitation.Status,
            ExpiresAt = invitation.ExpiresAt,
            CreatedAt = invitation.CreatedAt,
            InvitedBy = invitation.InvitedBy,
            Token = invitation.Token
        };
    }

    public async Task<InvitationDto?> GetInvitationByTokenAsync(string token)
    {
        var inv = await _invitationRepository.GetByTokenAsync(token);
        if (inv == null) return null;

        var role = await _roleRepository.GetByIdAsync(inv.RoleId);

        return new InvitationDto
        {
            Id = inv.Id,
            TenantId = inv.TenantId,
            Email = inv.Email,
            RoleName = role?.Name ?? string.Empty,
            Status = inv.Status,
            ExpiresAt = inv.ExpiresAt,
            CreatedAt = inv.CreatedAt
        };
    }

    public async Task<IEnumerable<InvitationDto>> GetPendingInvitationsAsync(string requestingUserId, Guid tenantId)
    {
        // Authorization check (owner/admin) kept to controller
        var pending = await _invitationRepository.GetPendingByTenantAsync(tenantId);
        var results = new List<InvitationDto>();
        foreach (var inv in pending)
        {
            var role = await _roleRepository.GetByIdAsync(inv.RoleId);
            results.Add(new InvitationDto
            {
                Id = inv.Id,
                TenantId = inv.TenantId,
                Email = inv.Email,
                RoleName = role?.Name ?? string.Empty,
                Status = inv.Status,
                ExpiresAt = inv.ExpiresAt,
                CreatedAt = inv.CreatedAt,
                InvitedBy = inv.InvitedBy,
                AcceptedBy = inv.AcceptedBy
            });
        }
        return results;
    }

    public async Task<bool> AcceptInvitationAsync(string token, string acceptingUserId)
    {
        var inv = await _invitationRepository.GetByTokenAsync(token);
        if (inv == null) return false;
        if (inv.ExpiresAt < DateTime.UtcNow) return false;
        if (inv.Status != "Pending") return false;

        // Check if user already exists
        var user = await _userRepository.GetByIdWithTenantAsync(acceptingUserId);
        if (user == null) return false;

        // Create membership
        var membership = new Membership
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TenantId = inv.TenantId,
            RoleId = inv.RoleId,
            Status = "Active",
            JoinedAt = DateTime.UtcNow,
            InvitedBy = inv.InvitedBy,
            CreatedAt = DateTime.UtcNow
        };

        await _membershipRepository.CreateMembershipAsync(membership);
        await _invitationRepository.AcceptInvitationAsync(inv.Id, acceptingUserId);

        return true;
    }

    private static string GenerateSecureToken()
    {
        // 32 bytes => 43 chars base64 URL-safe
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
