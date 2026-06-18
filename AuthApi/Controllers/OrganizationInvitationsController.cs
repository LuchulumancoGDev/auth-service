using Auth.Application.DTOs;
using Auth.Infrastructure.Repositories;
using Auth.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Controllers;

[ApiController]
[Route("api/organizations/{tenantId:guid}/[controller]")]
public class OrganizationInvitationsController : ControllerBase
{
    private readonly IInvitationService _invitationService;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IOrganizationInvitationRepository _invitationRepository;

    public OrganizationInvitationsController(
        IInvitationService invitationService,
        IMembershipRepository membershipRepository,
        IOrganizationInvitationRepository invitationRepository)
    {
        _invitationService = invitationService;
        _membershipRepository = membershipRepository;
        _invitationRepository = invitationRepository;
    }

    [HttpPost]
    [Authorize(Policy = "OrganizationAdminOnly")]
    public async Task<ActionResult<InvitationDto>> Create(Guid tenantId, [FromBody] CreateInvitationRequest request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var dto = await _invitationService.CreateInvitationAsync(userId, tenantId, request);
        return Ok(dto);
    }

    [HttpGet("pending")]
    [Authorize(Policy = "OrganizationAdminOnly")]
    public async Task<ActionResult<IEnumerable<InvitationDto>>> GetPending(Guid tenantId)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var pending = await _invitationService.GetPendingInvitationsAsync(userId, tenantId);
        return Ok(pending);
    }

    [HttpDelete("{invitationId:guid}")]
    [Authorize(Policy = "OrganizationAdminOnly")]
    public async Task<ActionResult> Revoke(Guid tenantId, Guid invitationId)
    {
        await _invitationRepository.ExpireInvitationAsync(invitationId);
        return NoContent();
    }
}
