namespace AuthApi.Controllers;

public class AuthResponse
{
    public string TokenType { get; set; }
    public string AccessToken { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public string RefreshToken { get; set; }
}