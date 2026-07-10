using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SoftSync.Common.Enums;
using SoftSync.DAL.Data;
using SoftSync.DAL.Entities;

namespace SoftSync.Presentation.Services;

/// <summary>
/// Applies pending migrations and seeds a demo account at runtime. The demo user
/// replaces the old <c>HasData</c> seed (Id=1) which is no longer valid for an
/// Identity user (needs a real PasswordHash/SecurityStamp).
/// </summary>
public static class DbInitializer
{
    public const string DemoEmail = "demo@softsync.local";
    public const string DemoPassword = "Demo@12345";

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        var db = sp.GetRequiredService<SoftSyncDbContext>();
        await db.Database.MigrateAsync();

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
            await userManager.CreateAsync(demo, DemoPassword);
        }
        else if (existing.ExperiencePoints == 0)
        {
            // Backfill XP for a demo account created before the level system existed.
            existing.ExperiencePoints = 320;
            await userManager.UpdateAsync(existing);
        }

        await BackfillGoalKeysAsync(db);
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
