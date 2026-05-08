namespace Auth.Domain.ValueObjects;

public class HashedRefreshToken
{
    public string Hash { get; private set; }
    public string? PlaintextForReturn { get; private set; }

    private HashedRefreshToken(string hash, string? plaintextForReturn = null)
    {
        Hash = hash;
        PlaintextForReturn = plaintextForReturn;
    }

    public static HashedRefreshToken Create(string tokenHash, string? plaintextForReturn = null)
    {
        return new HashedRefreshToken(tokenHash, plaintextForReturn);
    }

    public bool IsEmpty => string.IsNullOrWhiteSpace(Hash);
}
