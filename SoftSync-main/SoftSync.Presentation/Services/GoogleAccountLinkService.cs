using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SoftSync.DAL.Entities;

namespace SoftSync.Presentation.Services;

/// <summary>Links an existing user only from the protected Google external cookie.</summary>
public sealed class GoogleAccountLinkService(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn)
{
    public bool CanLink(ApplicationUser user, ExternalLoginInfo info)
    {
        // The callback obtains this principal through GetExternalLoginInfoAsync,
        // after the Google handler has validated OAuth state and fetched userinfo.
        if (info.LoginProvider != "Google" || info.Principal.Identity?.IsAuthenticated != true
            || string.IsNullOrWhiteSpace(info.ProviderKey)
            || info.ProviderKey != info.Principal.FindFirstValue(ClaimTypes.NameIdentifier)
            || !string.Equals(info.Principal.FindFirstValue("urn:google:email_verified"), "true", StringComparison.OrdinalIgnoreCase))
            return false;

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(user.Email)
            || users.NormalizeEmail(email) != users.NormalizeEmail(user.Email))
            return false;

        // Google is authoritative for Gmail and Workspace (verified email + hd).
        // For other addresses email_verified may outlive ownership of the mailbox:
        // https://developers.google.com/identity/sign-in/web/backend-auth
        return email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase)
            || !string.IsNullOrWhiteSpace(info.Principal.FindFirstValue("urn:google:hosted_domain"));
    }

    public async Task<SignInResult> LinkAndSignInAsync(ApplicationUser user, ExternalLoginInfo info)
    {
        if (!CanLink(user, info)) return SignInResult.Failed;
        if (!await signIn.CanSignInAsync(user)) return SignInResult.NotAllowed;
        if (users.SupportsUserLockout && await users.IsLockedOutAsync(user)) return SignInResult.LockedOut;
        // Linking is an account change. Do not bypass a local second factor even
        // when the incoming Google identity is otherwise trusted.
        if (users.SupportsUserTwoFactor && await users.GetTwoFactorEnabledAsync(user)) return SignInResult.TwoFactorRequired;

        var owner = await users.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
        if (owner is not null && owner.Id != user.Id) return SignInResult.Failed;
        if (owner is null)
        {
            // Keep a different Google subject already attached to this account.
            var logins = await users.GetLoginsAsync(user);
            if (logins.Any(login => login.LoginProvider == info.LoginProvider && login.ProviderKey != info.ProviderKey))
                return SignInResult.Failed;
            IdentityResult linked;
            try { linked = await users.AddLoginAsync(user, info); }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException
                { SqlState: PostgresErrorCodes.UniqueViolation, TableName: "AspNetUserLogins", ConstraintName: "PK_AspNetUserLogins" })
            {
                // Another callback may have claimed this subject concurrently.
                // Do not sign in from a failed insert; a fresh callback can retry.
                return SignInResult.Failed;
            }
            if (!linked.Succeeded) return SignInResult.Failed;
        }

        // Use the regular Identity path so lockout and sign-in restrictions are
        // checked again; never issue an application cookie using SignInAsync here.
        return await signIn.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey,
            isPersistent: true, bypassTwoFactor: false);
    }
}
