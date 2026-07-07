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
                Goal = "Cải thiện kỹ năng giao tiếp",
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
    }
}
