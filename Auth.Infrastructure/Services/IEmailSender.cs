namespace Auth.Infrastructure.Services;

public interface IEmailSender
{
    Task SendInvitationAsync(string toEmail, string token, Guid tenantId, string? invitedBy);
}
