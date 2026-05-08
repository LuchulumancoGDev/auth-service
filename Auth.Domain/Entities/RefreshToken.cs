namespace Auth.Domain.Entities;

public class RefreshToken : TenantEntity
{
    public string TokenHash { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
}
