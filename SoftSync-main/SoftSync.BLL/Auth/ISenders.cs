namespace SoftSync.BLL.Auth;

/// <summary>Sends transactional email. Named <c>IAppEmailSender</c> to avoid a
/// clash with <c>Microsoft.AspNetCore.Identity.UI.Services.IEmailSender</c>.</summary>
public interface IAppEmailSender
{
    Task SendAsync(string toEmail, string subject, string htmlBody);
}

/// <summary>Sends an SMS message via the configured provider (Twilio).</summary>
public interface IAppSmsSender
{
    Task SendAsync(string toPhoneNumber, string message);
}
