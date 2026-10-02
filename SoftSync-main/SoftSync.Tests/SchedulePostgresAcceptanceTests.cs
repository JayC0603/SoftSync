using Microsoft.EntityFrameworkCore;
using SoftSync.BLL.Interfaces;
using SoftSync.BLL.Services;
using SoftSync.DAL.Data;
using SoftSync.DAL.Entities;
using Xunit;

namespace SoftSync.Tests;

public sealed class SchedulePostgresAcceptanceTests
{
    [CvHttpAcceptanceFact]
    [Trait("Category", "ScheduleHttp")]
    public async Task Anonymous_schedule_request_redirects_to_login()
    {
        using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = true })
        {
            BaseAddress = new Uri(Environment.GetEnvironmentVariable("SOFTSYNC_CV_ACCEPTANCE_URL")!)
        };
        using var response = await client.GetAsync("/schedule");
        Assert.Equal("/Account/Login", response.RequestMessage!.RequestUri!.AbsolutePath);
    }

    [PostgreSqlAcceptanceFact]
    [Trait("Category", "SchedulePostgreSql")]
    public async Task Migration_and_fresh_context_preserve_private_vietnamese_task()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        var connection = Environment.GetEnvironmentVariable("SOFTSYNC_CV_ACCEPTANCE_DB")!;
        var options = new DbContextOptionsBuilder<SoftSyncDbContext>().UseNpgsql(connection).Options;
        var factory = new TestFactory(options);
        int firstId, secondId;
        await using (var db = factory.CreateDbContext())
        {
            await db.Database.MigrateAsync();
            var suffix = Guid.NewGuid().ToString("N");
            var first = new ApplicationUser { UserName = "schedule-a-" + suffix, Email = "schedule-a-" + suffix + "@example.invalid" };
            var second = new ApplicationUser { UserName = "schedule-b-" + suffix, Email = "schedule-b-" + suffix + "@example.invalid" };
            db.Users.AddRange(first, second);
            await db.SaveChangesAsync();
            firstId = first.Id; secondId = second.Id;
            await db.Database.OpenConnectionAsync();
            using var command = db.Database.GetDbConnection().CreateCommand();
            command.CommandText = "SELECT count(*) FROM pg_indexes WHERE schemaname='public' AND indexname='IX_ScheduleTasks_UserId_Date'";
            Assert.Equal(1L, await command.ExecuteScalarAsync());
            await db.Database.CloseConnectionAsync();
        }
        var user = new TestUser { Id = firstId };
        var service = new ScheduleService(factory, user);
        var date = new DateOnly(2026, 10, 5);
        var created = await service.CreateAsync(new ScheduleTaskInput("Ôn kỹ năng giao tiếp", "Chuẩn bị bài thuyết trình", date, new TimeOnly(9, 0), new TimeOnly(10, 0), false, ScheduleTaskPriority.High));
        // A fresh factory/context verifies database persistence without reading a tracked entity.
        var restarted = new ScheduleService(new TestFactory(options), user);
        Assert.Equal("Ôn kỹ năng giao tiếp", (await restarted.GetAsync(created.Id))!.Title);
        user.Id = secondId;
        Assert.Null(await restarted.GetAsync(created.Id));
        Assert.Empty(await restarted.GetPeriodAsync(date, date));
        Assert.False(await restarted.UpdateAsync(created.Id, new ScheduleTaskInput("Changed", null, date, null, null, true, ScheduleTaskPriority.Normal)));
        Assert.False(await restarted.SetCompletedAsync(created.Id, true));
        Assert.False(await restarted.DeleteAsync(created.Id));
        user.Id = firstId;
        Assert.True(await restarted.DeleteAsync(created.Id));
    }

    private sealed class TestUser : ICurrentScheduleUser
    {
        public int Id { get; set; }
        public Task<int> GetUserIdAsync() => Task.FromResult(Id);
    }
    private sealed class TestFactory(DbContextOptions<SoftSyncDbContext> options) : IDbContextFactory<SoftSyncDbContext>
    {
        public SoftSyncDbContext CreateDbContext() => new(options);
    }
}
