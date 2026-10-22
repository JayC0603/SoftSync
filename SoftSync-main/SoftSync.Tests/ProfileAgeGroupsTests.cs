using Microsoft.EntityFrameworkCore;
using SoftSync.BLL.Services;
using SoftSync.Common;
using SoftSync.DAL.Data;
using SoftSync.DAL.Entities;
using SoftSync.DAL.Repositories;
using Xunit;

namespace SoftSync.Tests;

public sealed class ProfileAgeGroupsTests
{
    [Theory]
    [InlineData(0, "")]
    [InlineData(17, "<18")]
    [InlineData(18, "18-22")]
    [InlineData(22, "18-22")]
    [InlineData(23, "23-27")]
    [InlineData(27, "23-27")]
    [InlineData(28, "28+")]
    [InlineData(31, "28+")]
    [InlineData(120, "28+")]
    public void Existing_ages_resolve_to_the_same_band_on_every_screen(int age, string expected)
    {
        Assert.Equal(expected, ProfileAgeGroups.ForAge(age));
        if (age > 0) Assert.Equal(age, ProfileAgeGroups.Select(expected, age));
    }

    [Fact]
    public async Task Onboarding_age_is_visible_in_settings_and_a_changed_band_persists()
    {
        var options = new DbContextOptionsBuilder<SoftSyncDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using (var db = new SoftSyncDbContext(options))
        {
            db.Users.Add(new ApplicationUser { Id = 100, FullName = "Learner" });
            await db.SaveChangesAsync();
            var users = new UserService(new UserRepository(db));
            var onboarding = (await users.GetUserByIdAsync(100))!;
            onboarding.Age = ProfileAgeGroups.Select("18-22", onboarding.Age);
            await users.UpdateProfileAsync(100, onboarding);
        }
        await using (var db = new SoftSyncDbContext(options))
        {
            var users = new UserService(new UserRepository(db));
            var settings = (await users.GetUserByIdAsync(100))!;
            Assert.Equal("18-22", ProfileAgeGroups.ForAge(settings.Age));
            settings.Age = ProfileAgeGroups.Select("28+", settings.Age);
            await users.UpdateSettingsAsync(100, settings);
        }
        await using var fresh = new SoftSyncDbContext(options);
        var profile = await new UserService(new UserRepository(fresh)).GetUserByIdAsync(100);
        Assert.Equal("28+", ProfileAgeGroups.ForAge(profile!.Age));
        Assert.Equal("wizard.age.28plus", ProfileAgeGroups.LabelKey(profile.Age));
    }

    [Fact]
    public void An_unknown_group_does_not_silently_become_thirty_years_old()
    {
        Assert.Throws<ArgumentException>(() => ProfileAgeGroups.Select("invalid", 31));
        Assert.Throws<ArgumentException>(() => ProfileAgeGroups.Select("", 0));
    }
}
