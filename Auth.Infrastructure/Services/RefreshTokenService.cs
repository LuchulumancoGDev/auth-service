using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Auth.Domain.Entities;
using Auth.Infrastructure.Persistence;

namespace Auth.Infrastructure.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly AppDbContext _db;
    private readonly ILogger<RefreshTokenService> _logger;

    public RefreshTokenService(AppDbContext db, ILogger<RefreshTokenService> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger;
    }

    public async Task<string> GenerateRefreshTokenAsync(ApplicationUser user, TimeSpan validFor)
    {
        if (user is null) throw new ArgumentNullException(nameof(user));

        var token = GenerateTokenString();
        var entity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            TokenHash = token,
            ExpiryDate = DateTime.UtcNow.Add(validFor),
            IsRevoked = false,
            UserId = user.Id
        };

        _db.RefreshTokens.Add(entity);
        await _db.SaveChangesAsync();

        return token;
    }

    public async Task<(bool Valid, RefreshToken TokenEntity)> ValidateRefreshTokenAsync(string userId, string refreshToken)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(refreshToken))
            return (false, null!);

        var token = await _db.RefreshTokens
            .FirstOrDefaultAsync(t => t.UserId == userId && t.TokenHash == refreshToken);

        if (token == null || token.IsRevoked || token.ExpiryDate <= DateTime.UtcNow)
            return (false, null!);

        return (true, token);
    }

    public async Task<string> RotateRefreshTokenAsync(string userId, string currentRefreshToken, TimeSpan validFor)
    {
        var (valid, existingToken) = await ValidateRefreshTokenAsync(userId, currentRefreshToken);
        if (!valid || existingToken == null)
            throw new InvalidOperationException("Invalid or expired refresh token.");

        existingToken.IsRevoked = true;
        existingToken.ExpiryDate = DateTime.UtcNow; // mark expired

        var newToken = GenerateTokenString();
        var newEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            TokenHash = newToken,
            ExpiryDate = DateTime.UtcNow.Add(validFor),
            IsRevoked = false,
            UserId = userId
        };

        _db.RefreshTokens.Add(newEntity);
        await _db.SaveChangesAsync();

        return newToken;
    }

    public async Task RevokeRefreshTokenAsync(string userId, string refreshToken)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(refreshToken)) return;

        var token = await _db.RefreshTokens
            .FirstOrDefaultAsync(t => t.UserId == userId && t.TokenHash == refreshToken);

        if (token == null) return;

        token.IsRevoked = true;
        await _db.SaveChangesAsync();
    }

    private static string GenerateTokenString()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        // URL-safe base64 without padding
        return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}