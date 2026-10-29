using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SoftSync.DAL.Data;
using SoftSync.DAL.Entities;

namespace SoftSync.Presentation.Services;

public sealed class EmailRegistrationService(UserManager<ApplicationUser> users, SoftSyncDbContext db)
{
    public const string DuplicateAccount = "DuplicateAccount";

    public async Task<IdentityResult> CreateAsync(ApplicationUser user, string password)
    {
        user.Email = user.Email?.Trim();
        user.UserName = user.Email;
        if (string.IsNullOrWhiteSpace(user.Email))
            return IdentityResult.Failed(users.ErrorDescriber.InvalidEmail(user.Email ?? ""));

        // UserManager applies the configured Identity normalizer to both lookups.
        if (await users.FindByEmailAsync(user.Email) is not null
            || await users.FindByNameAsync(user.Email) is not null)
            return Duplicate();

        try
        {
            var result = await users.CreateAsync(user, password);
            return result.Errors.Any(e => e.Code is "DuplicateEmail" or "DuplicateUserName")
                ? Duplicate() : result;
        }
        catch (DbUpdateException ex) when (IsDuplicateAccount(ex))
        {
            // Two registrations may pass the lookup together. The existing unique
            // UserNameIndex arbitrates the insert because email is the username.
            db.Entry(user).State = EntityState.Detached;
            return Duplicate();
        }
    }

    internal static bool IsDuplicateAccount(DbUpdateException ex) =>
        ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            TableName: "AspNetUsers",
            ConstraintName: "UserNameIndex" or "EmailIndex"
        };

    private static IdentityResult Duplicate() => IdentityResult.Failed(new IdentityError
    {
        Code = DuplicateAccount,
        Description = "Unable to register with these details. Sign in or recover your password if you already have an account."
    });
}
