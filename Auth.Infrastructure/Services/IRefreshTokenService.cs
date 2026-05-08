using Auth.Domain.Entities;

namespace Auth.Infrastructure.Services;

public interface IRefreshTokenService
{
    Task<string> GenerateRefreshTokenAsync(ApplicationUser user, TimeSpan validFor);
    Task<(bool Valid, RefreshToken TokenEntity)> ValidateRefreshTokenAsync(string userId, string refreshToken);
    Task<string> RotateRefreshTokenAsync(string userId, string currentRefreshToken, TimeSpan validFor);
    Task RevokeRefreshTokenAsync(string userId, string refreshToken);
}