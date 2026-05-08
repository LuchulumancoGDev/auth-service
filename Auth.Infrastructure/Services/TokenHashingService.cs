namespace Auth.Infrastructure.Services;

public interface ITokenHashingService
{
    string HashToken(string token);
    bool VerifyToken(string token, string hash);
}

public class TokenHashingService : ITokenHashingService
{
    public string HashToken(string token)
    {
        return BCrypt.Net.BCrypt.HashPassword(token);
    }

    public bool VerifyToken(string token, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(token, hash);
    }
}
