namespace Auth.Infrastructure.Services;

public class CurrentMembershipProvider : ICurrentMembershipProvider
{
    public MembershipInfo? CurrentMembership { get; set; }
    public IEnumerable<MembershipInfo> Memberships { get; set; } = Enumerable.Empty<MembershipInfo>();
}
