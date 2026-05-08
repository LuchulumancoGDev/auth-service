using Auth.Application.DTOs;
using Auth.Application.Services;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Auth.Infrastructure.Services;

public class SocialAuthenticationService : ISocialAuthenticationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SocialAuthenticationService> _logger;

    public SocialAuthenticationService(
        IHttpClientFactory httpClientFactory,
        ILogger<SocialAuthenticationService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<SocialUserInfo> ValidateGoogleTokenAsync(string accessToken, string? idToken, CancellationToken cancellationToken = default)
    {
        try
        {
            // If ID token is provided, validate and extract claims from it (OpenID Connect)
            if (!string.IsNullOrEmpty(idToken))
            {
                return await ValidateGoogleIdTokenAsync(idToken, cancellationToken);
            }

            // Otherwise, use the access token to get user info
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"https://www.googleapis.com/oauth2/v3/userinfo?access_token={accessToken}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException("Invalid Google access token");
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var userInfo = System.Text.Json.JsonSerializer.Deserialize<GoogleUserInfo>(json);

            if (userInfo == null)
            {
                throw new InvalidOperationException("Could not parse Google user info");
            }

            return new SocialUserInfo
            {
                Provider = "google",
                ProviderId = userInfo.Sub,
                Email = userInfo.Email,
                FirstName = userInfo.GivenName ?? string.Empty,
                LastName = userInfo.FamilyName ?? string.Empty,
                ProfilePicture = userInfo.Picture
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Google token");
            throw new InvalidOperationException("Failed to validate Google token", ex);
        }
    }

    private async Task<SocialUserInfo> ValidateGoogleIdTokenAsync(string idToken, CancellationToken cancellationToken)
    {
        // For production, use Google's token validation library
        // This is a simplified implementation
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(idToken);

        var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
        var emailVerified = jwtToken.Claims.FirstOrDefault(c => c.Type == "email_verified")?.Value == "true";
        var name = jwtToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
        var picture = jwtToken.Claims.FirstOrDefault(c => c.Type == "picture")?.Value;
        var sub = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(sub))
        {
            throw new InvalidOperationException("Invalid Google ID token");
        }

        var nameParts = (name ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return new SocialUserInfo
        {
            Provider = "google",
            ProviderId = sub,
            Email = email,
            FirstName = nameParts.FirstOrDefault() ?? string.Empty,
            LastName = nameParts.Skip(1).LastOrDefault() ?? string.Empty,
            ProfilePicture = picture
        };
    }

    public async Task<SocialUserInfo> ValidateMicrosoftTokenAsync(string accessToken, string? idToken, CancellationToken cancellationToken = default)
    {
        try
        {
            // If ID token is provided, validate and extract claims from it (OpenID Connect)
            if (!string.IsNullOrEmpty(idToken))
            {
                return await ValidateMicrosoftIdTokenAsync(idToken, cancellationToken);
            }

            // Otherwise, use the access token to get user info
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            var response = await client.GetAsync("https://graph.microsoft.com/v1.0/me", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException("Invalid Microsoft access token");
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var userInfo = System.Text.Json.JsonSerializer.Deserialize<MicrosoftUserInfo>(json);

            if (userInfo == null)
            {
                throw new InvalidOperationException("Could not parse Microsoft user info");
            }

            return new SocialUserInfo
            {
                Provider = "microsoft",
                ProviderId = userInfo.Id,
                Email = userInfo.Mail ?? userInfo.UserPrincipalName,
                FirstName = userInfo.GivenName ?? string.Empty,
                LastName = userInfo.Surname ?? string.Empty,
                ProfilePicture = null // Microsoft requires separate API call for photo
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Microsoft token");
            throw new InvalidOperationException("Failed to validate Microsoft token", ex);
        }
    }

    private async Task<SocialUserInfo> ValidateMicrosoftIdTokenAsync(string idToken, CancellationToken cancellationToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(idToken);

        var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value 
                    ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
        var name = jwtToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
        var sub = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(sub))
        {
            throw new InvalidOperationException("Invalid Microsoft ID token");
        }

        var nameParts = (name ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return new SocialUserInfo
        {
            Provider = "microsoft",
            ProviderId = sub,
            Email = email,
            FirstName = nameParts.FirstOrDefault() ?? string.Empty,
            LastName = nameParts.Skip(1).LastOrDefault() ?? string.Empty,
            ProfilePicture = null
        };
    }

    public async Task<SocialUserInfo> ValidateFacebookTokenAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(
                $"https://graph.facebook.com/me?fields=id,name,email,first_name,last_name,picture&access_token={accessToken}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException("Invalid Facebook access token");
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var userInfo = System.Text.Json.JsonSerializer.Deserialize<FacebookUserInfo>(json);

            if (userInfo == null)
            {
                throw new InvalidOperationException("Could not parse Facebook user info");
            }

            return new SocialUserInfo
            {
                Provider = "facebook",
                ProviderId = userInfo.Id,
                Email = userInfo.Email ?? string.Empty,
                FirstName = userInfo.FirstName ?? string.Empty,
                LastName = userInfo.LastName ?? string.Empty,
                ProfilePicture = userInfo.Picture?.Data?.Url
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Facebook token");
            throw new InvalidOperationException("Failed to validate Facebook token", ex);
        }
    }

    public async Task<SocialUserInfo> ValidateTwitterTokenAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            // Twitter API v2 requires OAuth 2.0 bearer token
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            
            var response = await client.GetAsync("https://api.twitter.com/2/users/me?user.fields=profile_image_url,email", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException("Invalid Twitter access token");
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var wrapper = System.Text.Json.JsonSerializer.Deserialize<TwitterResponseWrapper>(json);
            var userInfo = wrapper?.Data;

            if (userInfo == null)
            {
                throw new InvalidOperationException("Could not parse Twitter user info");
            }

            return new SocialUserInfo
            {
                Provider = "twitter",
                ProviderId = userInfo.Id,
                Email = userInfo.Email ?? string.Empty,
                FirstName = userInfo.Name ?? string.Empty,
                LastName = string.Empty, // Twitter doesn't provide last name separately
                ProfilePicture = userInfo.ProfileImageUrl
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Twitter token");
            throw new InvalidOperationException("Failed to validate Twitter token", ex);
        }
    }

    // Internal DTOs for deserialization
    private class GoogleUserInfo
    {
        public string Sub { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string GivenName { get; set; } = string.Empty;
        public string FamilyName { get; set; } = string.Empty;
        public string? Picture { get; set; }
    }

    private class MicrosoftUserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string? Mail { get; set; }
        public string UserPrincipalName { get; set; } = string.Empty;
        public string? GivenName { get; set; }
        public string? Surname { get; set; }
    }

    private class FacebookUserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Name { get; set; }
        public FacebookPicture? Picture { get; set; }
    }

    private class FacebookPicture
    {
        public FacebookPictureData? Data { get; set; }
    }

    private class FacebookPictureData
    {
        public string? Url { get; set; }
    }

    private class TwitterResponseWrapper
    {
        public TwitterUserInfo? Data { get; set; }
    }

    private class TwitterUserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? ProfileImageUrl { get; set; }
    }
}