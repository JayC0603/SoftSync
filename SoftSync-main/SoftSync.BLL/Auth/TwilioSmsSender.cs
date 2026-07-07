using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace SoftSync.BLL.Auth;

/// <summary>Sends SMS via Twilio. Throws a clear
/// <see cref="InvalidOperationException"/> when Twilio is not configured.</summary>
public class TwilioSmsSender : IAppSmsSender
{
    private readonly TwilioOptions _options;
    public TwilioSmsSender(IOptions<TwilioOptions> options) => _options = options.Value;

    public Task SendAsync(string toPhoneNumber, string message)
    {
        if (!_options.IsConfigured)
            throw new InvalidOperationException(
                "SMS chưa được cấu hình. Hãy đặt Twilio:AccountSid, Twilio:AuthToken, Twilio:FromNumber trong user-secrets.");

        TwilioClient.Init(_options.AccountSid, _options.AuthToken);
        return MessageResource.CreateAsync(
            to: new PhoneNumber(toPhoneNumber),
            from: new PhoneNumber(_options.FromNumber),
            body: message);
    }
}
