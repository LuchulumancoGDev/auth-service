using Auth.Application.DTOs;

namespace Auth.Application.Services;

public interface IAuthenticationService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<AuthResponse> SocialLoginAsync(SocialLoginRequest request, CancellationToken cancellationToken = default);
}
