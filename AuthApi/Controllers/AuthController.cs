using Auth.Application.DTOs;
using Auth.Application.Services;
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
}