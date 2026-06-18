namespace Auth.Infrastructure.Services;

public interface ICurrentMembershipProvider
{
    MembershipInfo? CurrentMembership { get; set; }
    IEnumerable<MembershipInfo> Memberships { get; set; }
}
