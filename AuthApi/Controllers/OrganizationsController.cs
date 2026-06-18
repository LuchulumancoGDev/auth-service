using Auth.Application.DTOs;
using Auth.Domain.Entities;
using Auth.Infrastructure.Repositories;
using Auth.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AuthApi.Controllers;

[ApiController]
[Authorize]
[Route("api/organizations")]
public class OrganizationsController : ControllerBase
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IOrganizationInvitationRepository _invitationRepository;
    private readonly IInvitationService _invitationService;
    private readonly IOrganizationService _organizationService;

    public OrganizationsController(
        IMembershipRepository membershipRepository,
        ITenantRepository tenantRepository,
        IOrganizationInvitationRepository invitationRepository,
        IInvitationService invitationService,
        IOrganizationService organizationService)
    {
        _membershipRepository = membershipRepository;
        _tenantRepository = tenantRepository;
        _invitationRepository = invitationRepository;
        _invitationService = invitationService;
        _organizationService = organizationService;
    }

    /// <summary>
    /// Gets the current user's active organization details.
    /// </summary>
    [HttpGet("current")]
    public async Task<ActionResult<OrganizationDto>> GetCurrentOrganization()
    {
        var tenantId = GetCurrentTenantId();
        if (tenantId == null) return Forbid();

        var tenant = await _tenantRepository.GetByIdAsync(tenantId.Value);
        if (tenant == null) return NotFound();

        return Ok(MapToOrganizationDto(tenant));
    }

    /// <summary>
    /// Gets organization details by ID. Only accessible to members of the organization.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrganizationDto>> GetOrganization(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        // Verify the user is a member of this organization
        if (!await _membershipRepository.IsMemberAsync(userId, id))
            return Forbid();

        var tenant = await _tenantRepository.GetByIdAsync(id);
        if (tenant == null) return NotFound();

        return Ok(MapToOrganizationDto(tenant));
    }

    /// <summary>
    /// Updates organization details. Only accessible to Owner/Admin roles.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<OrganizationDto>> UpdateOrganization(Guid id, [FromBody] UpdateOrganizationRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        // Verify user is Owner or Admin in this organization
        var isOwner = await _membershipRepository.IsOwnerAsync(userId, id);
        var isAdmin = await _membershipRepository.HasRoleInTenantAsync(userId, id, "Admin");
        if (!isOwner && !isAdmin)
            return Forbid();

        var tenant = await _tenantRepository.GetByIdAsync(id);
        if (tenant == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(request.Name))
            tenant.Name = request.Name;
        if (!string.IsNullOrWhiteSpace(request.Country))
            tenant.Country = request.Country;
        if (!string.IsNullOrWhiteSpace(request.LogoUrl))
            tenant.LogoUrl = request.LogoUrl;

        tenant.UpdatedAt = DateTime.UtcNow;
        _tenantRepository.Update(tenant);
        await _tenantRepository.SaveChangesAsync();

        return Ok(MapToOrganizationDto(tenant));
    }

    /// <summary>
    /// Gets all members of an organization.
    /// </summary>
    [HttpGet("{id:guid}/members")]
    public async Task<ActionResult<IEnumerable<MembershipDto>>> GetMembers(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        // Verify user is a member of this organization
        if (!await _membershipRepository.IsMemberAsync(userId, id))
            return Forbid();

        var members = await _membershipRepository.GetTenantMembersAsync(id);
        var memberDtos = members.Select(m => new MembershipDto
        {
            Id = m.Id,
            UserId = m.UserId,
            Email = m.User?.Email ?? string.Empty,
            FullName = m.User?.FullName ?? string.Empty,
            RoleId = m.RoleId,
            RoleName = m.Role?.Name ?? string.Empty,
            Status = m.Status,
            JoinedAt = m.JoinedAt
        });

        return Ok(memberDtos);
    }

    /// <summary>
    /// Gets pending invitations for an organization.
    /// </summary>
    [HttpGet("{id:guid}/invitations/pending")]
    public async Task<ActionResult<IEnumerable<InvitationDto>>> GetPendingInvitations(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        // Only Owner/Admin can view pending invitations
        var isOwner = await _membershipRepository.IsOwnerAsync(userId, id);
        var isAdmin = await _membershipRepository.HasRoleInTenantAsync(userId, id, "Admin");
        if (!isOwner && !isAdmin)
            return Forbid();

        var pending = await _invitationService.GetPendingInvitationsAsync(userId, id);
        return Ok(pending);
    }

    /// <summary>
    /// Lists all organizations the current user belongs to.
    /// </summary>
    [HttpGet("my-organizations")]
    public async Task<ActionResult<IEnumerable<OrganizationDto>>> GetMyOrganizations()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var memberships = await _membershipRepository.GetUserActiveMembershipsAsync(userId);
        var orgs = new List<OrganizationDto>();
        foreach (var membership in memberships)
        {
            orgs.Add(new OrganizationDto
            {
                Id = membership.TenantId,
                Name = membership.Tenant?.Name ?? string.Empty,
                Slug = membership.Tenant?.Slug ?? string.Empty,
                Type = membership.Tenant?.Type.ToString() ?? string.Empty,
                Role = membership.Role?.Name ?? string.Empty,
                MembershipId = membership.Id,
                IsOwner = membership.Tenant?.OwnerId == userId
            });
        }

        return Ok(orgs.OrderByDescending(o => o.IsOwner).ThenBy(o => o.Name));
    }

    private string? GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    private Guid? GetCurrentTenantId()
    {
        var claim = User.FindFirst("tenantId")?.Value;
        if (Guid.TryParse(claim, out var id) && id != Guid.Empty)
            return id;
        return null;
    }

    private static OrganizationDto MapToOrganizationDto(Tenant tenant) => new()
    {
        Id = tenant.Id,
        Name = tenant.Name,
        Slug = tenant.Slug,
        Plan = tenant.Plan,
        Status = tenant.Status,
        Country = tenant.Country,
        LogoUrl = tenant.LogoUrl,
        Type = tenant.Type.ToString(),
        IsActive = tenant.IsActive,
        CreatedAt = tenant.CreatedAt
    };
}