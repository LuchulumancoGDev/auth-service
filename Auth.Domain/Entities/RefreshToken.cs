namespace Auth.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }
    public string Token { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsRevoked { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
}
