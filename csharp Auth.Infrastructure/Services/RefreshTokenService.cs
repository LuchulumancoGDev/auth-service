using System.Security.Cryptography;
using System.Text;
using Auth.Domain.Entities;
using Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly AppDbContext _db;

    public RefreshTokenService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<string> GenerateRefreshTokenAsync(ApplicationUser user, TimeSpan validFor)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var hashed = Hash(token);

        var entity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = hashed,
            ExpiryDate = DateTime.UtcNow.Add(validFor),
            IsRevoked = false,
            UserId = user.Id
        };

        _db.RefreshTokens.Add(entity);
        await _db.SaveChangesAsync();

        // return raw token to client (store only hashed value)
        return token;
    }

    public async Task<(bool Valid, RefreshToken TokenEntity)> ValidateRefreshTokenAsync(string userId, string refreshToken)
    {
        var hashed = Hash(refreshToken);
        var tokenEntity = await _db.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.Token == hashed)
            .OrderByDescending(rt => rt.ExpiryDate)
            .FirstOrDefaultAsync();

        if (tokenEntity == null) return (false, null);
        if (tokenEntity.IsRevoked) return (false, tokenEntity);
        if (tokenEntity.ExpiryDate < DateTime.UtcNow) return (false, tokenEntity);

        return (true, tokenEntity);
    }

    public async Task<string> RotateRefreshTokenAsync(string userId, string currentRefreshToken, TimeSpan validFor)
    {
        var (valid, tokenEntity) = await ValidateRefreshTokenAsync(userId, currentRefreshToken);
        if (!valid) throw new InvalidOperationException("Invalid refresh token");

        // Revoke current
        tokenEntity.IsRevoked = true;
        _db.RefreshTokens.Update(tokenEntity);

        // Create new
        var user = await _db.Users.FindAsync(userId) ?? throw new InvalidOperationException("User not found");
        var newToken = await GenerateRefreshTokenAsync(user, validFor);

        await _db.SaveChangesAsync();
        return newToken;
    }

    public async Task RevokeRefreshTokenAsync(string userId, string refreshToken)
    {
        var hashed = Hash(refreshToken);
        var tokenEntity = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.UserId == userId && rt.Token == hashed);
        if (tokenEntity != null)
        {
            tokenEntity.IsRevoked = true;
            _db.RefreshTokens.Update(tokenEntity);
            await _db.SaveChangesAsync();
        }
    }

    private static string Hash(string value)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(value);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}