using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace Auth.Infrastructure.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _config;

    public SmtpEmailSender(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendInvitationAsync(string toEmail, string token, Guid tenantId, string? invitedBy)
    {
        var smtp = _config.GetSection("Smtp");
        var host = smtp["Host"] ?? throw new InvalidOperationException("Smtp:Host not configured");
        var port = int.Parse(smtp["Port"] ?? "25");
        var user = smtp["Username"];
        var pass = smtp["Password"];
        var from = smtp["From"] ?? user ?? "no-reply@example.com";
        var enableSsl = bool.Parse(smtp["EnableSsl"] ?? "true");

        var baseUrl = _config["Invitation:BaseUrl"] ?? "https://app.example.com/invite";
        var inviteUrl = $"{baseUrl}?token={Uri.EscapeDataString(token)}";

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl
        };

        if (!string.IsNullOrEmpty(user))
        {
            client.Credentials = new NetworkCredential(user, pass);
        }

        var mail = new MailMessage(from, toEmail)
        {
            Subject = "You're invited",
            Body = $"You were invited to join organization {tenantId} by {invitedBy}. Accept at: {inviteUrl}",
            IsBodyHtml = false
        };

        await client.SendMailAsync(mail);
    }
}
