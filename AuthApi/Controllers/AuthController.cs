using Auth.Application.DTOs;
using Auth.Application.Services;
using Auth.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;

    public AuthController(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _authenticationService.RegisterAsync(request, cancellationToken);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authenticationService.LoginAsync(request, cancellationToken);
        
        if (!result.IsSuccess)
        {
            return Unauthorized(result);
        }
        
        return Ok(result);
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _authenticationService.RefreshTokenAsync(request.RefreshToken, cancellationToken);
        
        if (!result.IsSuccess)
        {
            return Unauthorized(result);
        }
        
        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        // In a real implementation, you would revoke the current refresh token
        // For now, we'll just return success
        return Ok(new { message = "Logged out successfully" });
    }

    [HttpPost("social-login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> SocialLogin([FromBody] SocialLoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authenticationService.SocialLoginAsync(request, cancellationToken);
        
        if (!result.IsSuccess)
        {
            return Unauthorized(result);
        }
        
        return Ok(result);
    }

    [HttpPost("invitations")]
    [Authorize]
    public async Task<ActionResult<InvitationDto>> CreateInvitation([FromBody] CreateInvitationRequest request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!
                     ?? throw new InvalidOperationException("User id not found");

        // Tenant resolution: assume tenantId comes from user's claims for now
        var tenantClaim = User.FindFirst("tenantId")?.Value;
        if (tenantClaim == null) return Forbid();
        var tenantId = Guid.Parse(tenantClaim);

        var invitation = await HttpContext.RequestServices.GetRequiredService<IInvitationService>()
            .CreateInvitationAsync(userId, tenantId, request);

        return Ok(invitation);
    }

    [HttpPost("invitations/accept")]
    [AllowAnonymous]
    public async Task<ActionResult> AcceptInvitation([FromBody] AcceptInvitationRequest request)
    {
        // Accept token; if user is authenticated, use their id, otherwise return error for now
        string? userId = User.Identity?.IsAuthenticated == true
            ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            : null;

        if (string.IsNullOrEmpty(userId)) return BadRequest(new { message = "Must be authenticated to accept invitation" });

        var result = await HttpContext.RequestServices.GetRequiredService<IInvitationService>()
            .AcceptInvitationAsync(request.Token, userId);

        if (!result) return BadRequest(new { message = "Invalid or expired token" });
        return Ok(new { message = "Invitation accepted" });
    }
}