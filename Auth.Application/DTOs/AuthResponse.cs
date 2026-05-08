namespace Auth.Application.DTOs;

public class AuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public UserDto User { get; set; }
    public JwtTokenResponse Token { get; set; }
}
