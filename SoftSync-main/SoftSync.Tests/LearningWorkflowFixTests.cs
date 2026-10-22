using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SoftSync.BLL.Services;
using SoftSync.BLL.Services.Fake;
using SoftSync.Common;
using SoftSync.Common.Dtos;
using SoftSync.DAL.Data;
using SoftSync.DAL.Entities;
using SoftSync.DAL.Repositories;
using Xunit;

namespace SoftSync.Tests;

public sealed class LearningWorkflowFixTests
{
    [Fact]
    public async Task Recommended_roadmap_item_matches_save_permissions_and_reload()
    {
        await using var db = await DatabaseAsync();
        var users = new UserService(new UserRepository(db));
        var timeSkill = await db.Skills.SingleAsync(x => x.Name == "Time Management");
        await users.AddSkillSelectionsAsync(100, [1, timeSkill.Id]);
        var service = new RoadmapService(new RoadmapRepository(db), new FakeAiRoadmapService(),
            new UserRepository(db), new AssessmentRepository(db), new SkillRepository(db));
        await service.GetUserRoadmapAsync(100, 100);
        // Imported week numbers can interleave skills; access uses curriculum skill order.
        foreach (var row in await db.RoadmapItems.Where(x => x.UserId == 100).ToListAsync())
            if (row.Title.Contains("giao tiếp", StringComparison.OrdinalIgnoreCase)) row.WeekNumber += 10;
        await db.SaveChangesAsync();
        var plan = await service.GetUserRoadmapAsync(100, 100);
        var recommended = plan.Items.Single(x => x.Id == plan.NextRecommendedItemId);
        Assert.Equal("Giao tiếp", recommended.SkillName);
        Assert.True(await service.MarkVideoCompleteAsync(recommended.Id, 100));
        var reloaded = await service.GetUserRoadmapAsync(100, 100);
        var saved = reloaded.Items.Single(x => x.Id == recommended.Id);
        Assert.Equal(1, saved.CompletedActivityCount);
        Assert.True(saved.IsVideoCompleted);
        Assert.Equal(SoftSync.Common.Enums.RoadmapActivityType.LessonScript, reloaded.NextRecommendedActivity);
    }

    [Fact]
    public async Task Curriculum_only_publishes_supported_lessons()
    {
        var plan = await new FakeAiRoadmapService().GenerateRoadmapAsync(100,
            ["Communication", "Time Management", "Critical Thinking"]);
        Assert.Equal(4, plan.Items.Count);
        Assert.Equal(Enumerable.Range(1, 4), plan.Items.Select(x => x.WeekNumber));
        Assert.DoesNotContain(plan.Items, x => x.Title.Contains("Tuần 3") || x.Title.Contains("Tuần 4") || x.Title.Contains("Tư duy"));
    }

    [Fact]
    public void Activity_days_use_saved_dates_and_streak_allows_today_to_be_unfinished()
    {
        var today = new DateOnly(2026, 10, 19);
        var days = LearningActivity.Days([
            new() { VideoCompletedAtUtc = new DateTime(2026, 10, 18, 8, 0, 0), ScriptCompletedAtUtc = new DateTime(2026, 10, 18, 9, 0, 0) },
            new() { PracticeCompletedAtUtc = new DateTime(2026, 10, 17, 8, 0, 0) },
            new() { IsCompleted = true }]);
        Assert.Equal(2, days.Count);
        Assert.Equal(2, LearningActivity.Streak(days, today));
        Assert.Equal(0, LearningActivity.Streak(days, today.AddDays(1)));
        days.Add(today);
        Assert.Equal(3, LearningActivity.Streak(days, today));
    }

    [Fact]
    public async Task Changing_selected_skills_adds_lessons_without_replacing_progress()
    {
        await using var db = await DatabaseAsync();
        var users = new UserService(new UserRepository(db));
        await users.AddSkillSelectionsAsync(100, [1]);
        var service = new RoadmapService(new RoadmapRepository(db), new FakeAiRoadmapService(),
            new UserRepository(db), new AssessmentRepository(db), new SkillRepository(db));
        var original = await service.GetUserRoadmapAsync(100, 100);
        Assert.Equal(2, original.Items.Count);
        var firstId = original.Items[0].Id;
        Assert.True(await service.MarkVideoCompleteAsync(firstId, 100));
        var timeManagement = await db.Skills.SingleAsync(x => x.Name == "Time Management");
        await users.AddSkillSelectionsAsync(100, [1, timeManagement.Id]);
        var expanded = await service.GetUserRoadmapAsync(100, 100);
        Assert.Equal(4, expanded.Items.Count);
        Assert.True(expanded.Items.Single(x => x.Id == firstId).IsVideoCompleted);
        await service.GetUserRoadmapAsync(100, 100);
        Assert.Equal(4, await db.RoadmapItems.CountAsync(x => x.UserId == 100));
    }

    [Fact]
    public async Task Mentor_requests_are_private_and_admin_replies_reach_only_the_owner()
    {
        await using var db = await DatabaseAsync();
        var mentor = await db.Mentors.FirstAsync();
        db.MentorSupportRequests.Add(new MentorSupportRequest { UserId = 100, MentorId = mentor.Id,
            Goal = "Prepare feedback", Context = "Help me prepare a respectful feedback conversation.",
            Skill = "Private skill", ShareLearningContext = false, PreferredContact = "In-app" });
        await db.SaveChangesAsync();
        var changes = new NotificationChanges(NullLogger<NotificationChanges>.Instance);
        var service = new MentorRequestWorkflow(db, changes);
        var owned = Assert.Single(await service.ListAsync(Caller(100)));
        Assert.Empty(owned.Skill);
        Assert.Empty(await service.ListAsync(Caller(101)));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.ListAsync(Caller(100), true));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.RespondAsync(Caller(100, "Admin"), owned.Id, "Resolved", "Advice"));
        Assert.True(await service.RespondAsync(Caller(102, "Admin"), owned.Id, "Resolved", "Try a specific example and agree the next step."));
        Assert.Equal("Resolved", (await db.MentorSupportRequests.FindAsync(owned.Id))!.Status);
        var notification = Assert.Single(await db.UserNotifications.ToListAsync());
        Assert.Equal(100, notification.UserId);
        Assert.False(await service.RespondAsync(Caller(102, "Admin"), owned.Id, "Resolved", "Duplicate reply"));
        Assert.Single(await db.Announcements.ToListAsync());
        db.UserRoles.RemoveRange(db.UserRoles.Where(x => x.UserId == 102));
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.ListAsync(Caller(102, "Admin"), true));
    }

    private static ClaimsPrincipal Caller(int id, string? role = null)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, id.ToString()) };
        if (role is not null) claims.Add(new(ClaimTypes.Role, role));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
    }

    private static async Task<SoftSyncDbContext> DatabaseAsync()
    {
        var db = new SoftSyncDbContext(new DbContextOptionsBuilder<SoftSyncDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        await db.Database.EnsureCreatedAsync();
        db.Users.AddRange(new ApplicationUser { Id = 100, UserName = "learner" },
            new ApplicationUser { Id = 101, UserName = "other" }, new ApplicationUser { Id = 102, UserName = "admin" });
        db.Roles.Add(new IdentityRole<int> { Id = 102, Name = "Admin", NormalizedName = "ADMIN" });
        db.UserRoles.Add(new IdentityUserRole<int> { UserId = 102, RoleId = 102 });
        await db.SaveChangesAsync();
        return db;
    }
}
