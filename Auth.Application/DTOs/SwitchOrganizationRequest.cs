namespace Auth.Application.DTOs;

public class SwitchOrganizationRequest
{
    /// <summary>
    /// The membership ID to switch to.
    /// User must own this membership and it must be active.
    /// </summary>
    public Guid MembershipId { get; set; }
}