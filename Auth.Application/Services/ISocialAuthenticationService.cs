using Auth.Application.DTOs;

namespace Auth.Application.Services;

public interface ISocialAuthenticationService
{
    Task<SocialUserInfo> ValidateGoogleTokenAsync(string accessToken, string? idToken, CancellationToken cancellationToken = default);
    Task<SocialUserInfo> ValidateMicrosoftTokenAsync(string accessToken, string? idToken, CancellationToken cancellationToken = default);
    Task<SocialUserInfo> ValidateFacebookTokenAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<SocialUserInfo> ValidateTwitterTokenAsync(string accessToken, CancellationToken cancellationToken = default);
}