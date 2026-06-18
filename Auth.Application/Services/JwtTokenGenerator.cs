using Auth.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Auth.Application.Services;

public interface IJwtTokenGenerator
{
    // memberships: optional list of active memberships to embed in token
    string GenerateAccessToken(ApplicationUser user, IEnumerable<Auth.Domain.Entities.Membership>? memberships = null);
    string GenerateRefreshToken();
}

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(ApplicationUser user, IEnumerable<Auth.Domain.Entities.Membership>? memberships = null)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim("email", user.Email ?? string.Empty),
            new Claim("role", user.UserType.ToString())
        };

        if (memberships != null && memberships.Any())
        {
            // Use first active membership as primary tenant context
            var primaryMembership = memberships.First();
            claims.Add(new Claim("tenantId", primaryMembership.TenantId.ToString()));
            claims.Add(new Claim("membership_id", primaryMembership.Id.ToString()));
            if (primaryMembership.Role != null)
            {
                claims.Add(new Claim("role_name", primaryMembership.Role.Name));
            }

            // embed all memberships as a JSON array in a single claim
            var membershipDtos = memberships.Select(m => new
            {
                membershipId = m.Id,
                tenantId = m.TenantId,
                roleId = m.RoleId,
                roleName = m.Role?.Name ?? string.Empty,
                status = m.Status
            });
            var json = System.Text.Json.JsonSerializer.Serialize(membershipDtos);
            claims.Add(new Claim("memberships", json));
        }
        else
        {
            // Fallback: user has no memberships (personal account), tenantId = empty
            claims.Add(new Claim("tenantId", Guid.Empty.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
