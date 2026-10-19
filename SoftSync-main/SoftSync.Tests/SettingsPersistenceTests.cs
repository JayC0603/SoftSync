using Microsoft.EntityFrameworkCore;
using SoftSync.BLL.Services;
using SoftSync.Common.Dtos;
using SoftSync.Common.Enums;
using SoftSync.DAL.Data;
using SoftSync.DAL.Entities;
using SoftSync.DAL.Repositories;
using Xunit;

namespace SoftSync.Tests;

public sealed class SettingsPersistenceTests
{
    private static DbContextOptions<SoftSyncDbContext> Options() =>
        new DbContextOptionsBuilder<SoftSyncDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

    [Fact]
    public async Task Account_learning_and_appearance_changes_survive_a_fresh_context_without_invalidating_login()
    {
        var options = Options();
        await using (var db = new SoftSyncDbContext(options))
        {
            await db.Database.EnsureCreatedAsync();
            db.Users.Add(new ApplicationUser
            {
                Id = 100, FullName = "Original", Email = "account@example.test",
                UserName = "account@example.test", SecurityStamp = "signed-in-stamp",
                PasswordHash = "existing-password", ExperiencePoints = 320, Role = UserRole.Student
            });
            await db.SaveChangesAsync();
            var service = new UserService(new UserRepository(db));
            var dto = (await service.GetUserByIdAsync(100))!;
            dto.FullName = "  Updated name  ";
            dto.DisplayName = "  Public name  ";
            dto.Age = 25;
            dto.Gender = Gender.Female;
            dto.Goal = "wizard.goal.teamwork";
            dto.CurrentLevel = LearningLevel.Intermediate;
            dto.DailyStudyMinutes = 45;
            dto.StudyDaysPerWeek = 4;
            dto.PreferredStudyTime = StudyTime.Evening;
            dto.PreferredLanguage = "vi";
            dto.Theme = ThemePreference.Dark;
            dto.ReduceMotion = true;
            await service.UpdateSettingsAsync(100, dto);
            await service.AddSkillSelectionsAsync(100, [1, 3]);
            await service.AddSkillSelectionsAsync(100, [3]);
        }

        await using var fresh = new SoftSyncDbContext(options);
        var saved = await fresh.Users.Include(x => x.SkillSelections).SingleAsync(x => x.Id == 100);
        Assert.Equal("Updated name", saved.FullName);
        Assert.Equal("Public name", saved.DisplayName);
        Assert.Equal(25, saved.Age);
        Assert.Equal(Gender.Female, saved.Gender);
        Assert.Equal("wizard.goal.teamwork", saved.Goal);
        Assert.Equal(LearningLevel.Intermediate, saved.CurrentLevel);
        Assert.Equal(45, saved.DailyStudyMinutes);
        Assert.Equal(4, saved.StudyDaysPerWeek);
        Assert.Equal(StudyTime.Evening, saved.PreferredStudyTime);
        Assert.Equal("vi", saved.PreferredLanguage);
        Assert.Equal(ThemePreference.Dark, saved.Theme);
        Assert.True(saved.ReduceMotion);
        Assert.Equal(3, Assert.Single(saved.SkillSelections).SkillId);
        Assert.Equal("signed-in-stamp", saved.SecurityStamp);
        Assert.Equal("existing-password", saved.PasswordHash);
        Assert.Equal("account@example.test", saved.Email);
        Assert.Equal(320, saved.ExperiencePoints);
    }

    [Fact]
    public async Task Invalid_account_data_is_rejected_before_any_profile_fields_change()
    {
        await using var db = new SoftSyncDbContext(Options());
        db.Users.Add(new ApplicationUser { Id = 100, FullName = "Original", DisplayName = "Existing" });
        await db.SaveChangesAsync();
        var service = new UserService(new UserRepository(db));
        var invalid = new UserDto { FullName = " ", DisplayName = "Changed", Age = 20 };
        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateSettingsAsync(100, invalid));
        Assert.Equal("Original", (await db.Users.FindAsync(100))!.FullName);
        Assert.Equal("Existing", (await db.Users.FindAsync(100))!.DisplayName);
        invalid.FullName = "Valid";
        invalid.DisplayName = new string('x', 61);
        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateSettingsAsync(100, invalid));
    }

    [Fact]
    public async Task A_missing_account_cannot_report_a_successful_save()
    {
        await using var db = new SoftSyncDbContext(Options());
        var service = new UserService(new UserRepository(db));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateSettingsAsync(100,
            new UserDto { FullName = "Valid", Age = 20 }));
    }
}
