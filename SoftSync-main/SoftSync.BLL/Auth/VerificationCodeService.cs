using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SoftSync.Common.Enums;
using SoftSync.DAL.Data;
using SoftSync.DAL.Entities;

namespace SoftSync.BLL.Auth;

/// <summary>Outcome of validating a submitted verification code.</summary>
public enum VerificationResult
{
    Success,
    NotFound,
    Expired,
    TooManyAttempts,
    Invalid
}

/// <summary>
/// Issues and validates short numeric codes for OTP login, phone confirmation
/// and password reset. Codes are stored hashed (SHA-256), expire after a few
/// minutes, and lock after too many wrong attempts.
/// </summary>
public class VerificationCodeService
{
    private const int CodeLength = 6;
    private const int ExpiryMinutes = 10;
    private const int MaxAttempts = 5;

    private readonly SoftSyncDbContext _db;
    private readonly IAppSmsSender _sms;
    private readonly IAppEmailSender _email;

    public VerificationCodeService(SoftSyncDbContext db, IAppSmsSender sms, IAppEmailSender email)
    {
        _db = db;
        _sms = sms;
        _email = email;
    }

    /// <summary>
    /// Generate a code, store its hash, and dispatch it to <paramref name="destination"/>
    /// via SMS or email depending on <paramref name="purpose"/>. Any previous
    /// unconsumed codes for the same destination+purpose are invalidated first.
    /// </summary>
    public async Task IssueAsync(VerificationPurpose purpose, string destination, int? userId)
    {
        // Invalidate prior outstanding codes for this destination + purpose.
        var stale = await _db.VerificationCodes
            .Where(v => !v.Consumed && v.Purpose == purpose && v.Destination == destination)
            .ToListAsync();
        foreach (var s in stale) s.Consumed = true;

        var code = GenerateCode();
        _db.VerificationCodes.Add(new VerificationCode
        {
            UserId = userId,
            Purpose = purpose,
            Destination = destination,
            CodeHash = Hash(code),
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(ExpiryMinutes),
            CreatedAtUtc = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        var body = $"Mã xác nhận SoftSync của bạn là {code}. Mã hết hạn sau {ExpiryMinutes} phút.";
        if (purpose is VerificationPurpose.PhoneLoginOtp
            or VerificationPurpose.PhoneNumberConfirmation
            or VerificationPurpose.PasswordResetSms)
        {
            await _sms.SendAsync(destination, body);
        }
        else
        {
            await _email.SendAsync(destination, "SoftSync – Mã xác nhận", $"<p>{body}</p>");
        }
    }

    /// <summary>
    /// Check a submitted code against the latest outstanding code for the
    /// destination+purpose. On success the code is consumed. Wrong attempts
    /// increment a counter that locks the code after <see cref="MaxAttempts"/>.
    /// </summary>
    public async Task<VerificationResult> VerifyAsync(VerificationPurpose purpose, string destination, string code)
    {
        var entry = await _db.VerificationCodes
            .Where(v => !v.Consumed && v.Purpose == purpose && v.Destination == destination)
            .OrderByDescending(v => v.CreatedAtUtc)
            .FirstOrDefaultAsync();

        if (entry == null) return VerificationResult.NotFound;

        if (entry.ExpiresAtUtc < DateTime.UtcNow)
        {
            entry.Consumed = true;
            await _db.SaveChangesAsync();
            return VerificationResult.Expired;
        }

        if (entry.AttemptCount >= MaxAttempts)
        {
            entry.Consumed = true;
            await _db.SaveChangesAsync();
            return VerificationResult.TooManyAttempts;
        }

        if (!FixedTimeEquals(entry.CodeHash, Hash(code)))
        {
            entry.AttemptCount++;
            await _db.SaveChangesAsync();
            return VerificationResult.Invalid;
        }

        entry.Consumed = true;
        await _db.SaveChangesAsync();
        return VerificationResult.Success;
    }

    private static string GenerateCode()
    {
        // Uniformly-distributed CodeLength-digit number (leading zeros kept).
        var max = (int)Math.Pow(10, CodeLength);
        var value = RandomNumberGenerator.GetInt32(0, max);
        return value.ToString(new string('0', CodeLength));
    }

    private static string Hash(string code)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(code));
        return Convert.ToHexString(bytes);
    }

    private static bool FixedTimeEquals(string a, string b) =>
        CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(a), Encoding.UTF8.GetBytes(b));
}
