namespace Auth.Application.DTOs;

public class SocialLoginRequest
{
    public string Provider { get; set; } = string.Empty; // "google", "microsoft", "facebook", "twitter"
    public string AccessToken { get; set; } = string.Empty;
    public string? IdToken { get; set; } // For OpenID Connect providers (Google, Microsoft)
}