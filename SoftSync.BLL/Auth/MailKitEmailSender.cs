using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace SoftSync.BLL.Auth;

/// <summary>Sends email over SMTP using MailKit. Throws a clear
/// <see cref="InvalidOperationException"/> when SMTP is not configured.</summary>
public class MailKitEmailSender : IAppEmailSender
{
    private readonly SmtpOptions _options;
    public MailKitEmailSender(IOptions<SmtpOptions> options) => _options = options.Value;

    public async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        if (!_options.IsConfigured)
            throw new InvalidOperationException(
                "Email chưa được cấu hình. Hãy đặt Smtp:Host, Smtp:FromEmail (và Smtp:User/Smtp:Password nếu cần) trong user-secrets.");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        var socketOptions = _options.UseStartTls
            ? SecureSocketOptions.StartTls
            : SecureSocketOptions.Auto;
        await client.ConnectAsync(_options.Host, _options.Port, socketOptions);
        if (!string.IsNullOrWhiteSpace(_options.User))
            await client.AuthenticateAsync(_options.User, _options.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
