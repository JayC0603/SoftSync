using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SoftSync.BLL.Auth;
using SoftSync.Common.Enums;
using SoftSync.DAL.Data;
using SoftSync.DAL.Entities;

namespace SoftSync.Presentation.Services;

/// <summary>
/// Applies pending migrations and, in Development only, seeds a demo account.
/// The demo user replaces the old <c>HasData</c> seed (Id=1), which is no longer
/// valid for an Identity user (needs a real PasswordHash/SecurityStamp).
/// </summary>
public static class DbInitializer
{
    public const string DemoEmail = "demo@softsync.local";
    public const string DemoPassword = "Demo@12345";

    public static async Task SeedAsync(IServiceProvider services, bool seedDemoAccount)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        var db = sp.GetRequiredService<SoftSyncDbContext>();
        await db.Database.MigrateAsync();

        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole<int>>>();
        foreach (var roleName in CourseAuthorization.Roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole<int>(roleName));
                if (!roleResult.Succeeded)
                    throw new InvalidOperationException($"Could not create required Identity role '{roleName}': {string.Join(", ", roleResult.Errors.Select(error => error.Code))}");
            }
        }

        await BootstrapAdminAsync(sp);

        if (seedDemoAccount)
        {
            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
            var existing = await userManager.FindByEmailAsync(DemoEmail);
            if (existing is null)
            {
                var demo = new ApplicationUser
                {
                    UserName = DemoEmail,
                    Email = DemoEmail,
                    EmailConfirmed = true,
                    FullName = "Nguyễn Văn A",
                    Age = 20,
                    Role = UserRole.Student,
                    Goal = "wizard.goal.communication",
                    ExperiencePoints = 320, // demo: ~level 3
                    CreatedAt = DateTime.UtcNow
                };
                var createResult = await userManager.CreateAsync(demo, DemoPassword);
                if (!createResult.Succeeded)
                    throw new InvalidOperationException($"Could not create the development demo account: {string.Join(", ", createResult.Errors.Select(error => error.Code))}");
                await userManager.AddToRoleAsync(demo, CourseAuthorization.UserRole);
            }
            else if (existing.ExperiencePoints == 0)
            {
                // Backfill XP for a demo account created before the level system existed.
                existing.ExperiencePoints = 320;
                await userManager.UpdateAsync(existing);
            }

            if (existing is not null && !await userManager.IsInRoleAsync(existing, CourseAuthorization.UserRole))
                await userManager.AddToRoleAsync(existing, CourseAuthorization.UserRole);
        }

        await MigrateLegacyStudentRolesAsync(sp);

        await BackfillGoalKeysAsync(db);
    }

    private static async Task MigrateLegacyStudentRolesAsync(IServiceProvider services)
    {
        var roles = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
        if (!await roles.RoleExistsAsync(CourseAuthorization.LegacyStudentRole)) return;

        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        foreach (var user in await users.GetUsersInRoleAsync(CourseAuthorization.LegacyStudentRole))
        {
            if (!await users.IsInRoleAsync(user, CourseAuthorization.UserRole))
            {
                var added = await users.AddToRoleAsync(user, CourseAuthorization.UserRole);
                if (!added.Succeeded)
                    throw new InvalidOperationException($"Could not migrate user {user.Id} to the User role: {string.Join(", ", added.Errors.Select(error => error.Code))}");
            }
            var removed = await users.RemoveFromRoleAsync(user, CourseAuthorization.LegacyStudentRole);
            if (!removed.Succeeded)
                throw new InvalidOperationException($"Could not remove legacy Student role from user {user.Id}: {string.Join(", ", removed.Errors.Select(error => error.Code))}");
        }
    }

    private static async Task BootstrapAdminAsync(IServiceProvider services)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        var email = configuration["BootstrapAdmin:Email"]?.Trim();
        var password = configuration["BootstrapAdmin:Password"];
        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(password)) return;
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("BootstrapAdmin requires both Email and Password.");

        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await users.FindByEmailAsync(email);
        if (admin is not null)
        {
            // An existing account may have registered this unconfirmed email first.
            // Never promote it just because its address matches configuration.
            if (!await users.IsInRoleAsync(admin, CourseAuthorization.AdminRole))
                throw new InvalidOperationException("BootstrapAdmin email belongs to an existing non-Admin account. Resolve the account manually before starting the app.");
            return;
        }

        admin = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = "SoftSync Administrator",
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow
        };
        var created = await users.CreateAsync(admin, password);
        if (!created.Succeeded)
            throw new InvalidOperationException($"Could not create bootstrap Admin: {string.Join(", ", created.Errors.Select(error => error.Code))}");

        var assigned = await users.AddToRoleAsync(admin, CourseAuthorization.AdminRole);
        if (!assigned.Succeeded)
            throw new InvalidOperationException($"Could not assign bootstrap Admin role: {string.Join(", ", assigned.Errors.Select(error => error.Code))}");
    }

    // Older builds stored the goal as resolved text (e.g. "Cải thiện giao tiếp")
    // instead of a translation key, so it couldn't switch languages. Map any known
    // legacy text (either language) back to its key so display re-localizes.
    private static readonly Dictionary<string, string> LegacyGoalText = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Improve communication"] = "wizard.goal.communication",
        ["Cải thiện giao tiếp"] = "wizard.goal.communication",
        ["Cải thiện kỹ năng giao tiếp"] = "wizard.goal.communication",
        ["Prepare for internship"] = "wizard.goal.internship",
        ["Chuẩn bị cho thực tập"] = "wizard.goal.internship",
        ["Build leadership skills"] = "wizard.goal.leadership",
        ["Xây dựng kỹ năng lãnh đạo"] = "wizard.goal.leadership",
        ["Improve teamwork"] = "wizard.goal.teamwork",
        ["Cải thiện làm việc nhóm"] = "wizard.goal.teamwork",
    };

    private static async Task BackfillGoalKeysAsync(SoftSyncDbContext db)
    {
        var users = await db.Users.Where(u => u.Goal != "" && !u.Goal.StartsWith("wizard.goal.")).ToListAsync();
        var changed = false;
        foreach (var u in users)
        {
            if (LegacyGoalText.TryGetValue(u.Goal.Trim(), out var key))
            {
                u.Goal = key;
                changed = true;
            }
        }
        if (changed) await db.SaveChangesAsync();
    }
}
