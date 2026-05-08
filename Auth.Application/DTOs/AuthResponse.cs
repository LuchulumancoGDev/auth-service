namespace Auth.Application.DTOs;

public class AuthResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public UserDto? User { get; set; }
    public JwtTokenResponse? Token { get; set; }

    public static AuthResponse CreateSuccess(string message, UserDto user, JwtTokenResponse token)
    {
        return new AuthResponse
        {
            IsSuccess = true,
            Message = message,
            User = user,
            Token = token
        };
    }

    public static AuthResponse CreateError(string message)
    {
        return new AuthResponse
        {
            IsSuccess = false,
            Message = message
        };
    }
}