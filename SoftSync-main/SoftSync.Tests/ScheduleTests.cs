using Microsoft.EntityFrameworkCore;
using SoftSync.BLL.Interfaces;
using SoftSync.BLL.Services;
using SoftSync.DAL.Data;
using SoftSync.DAL.Entities;
using Xunit;

namespace SoftSync.Tests;

public sealed class ScheduleTests
{
    private static readonly DateOnly Oct5 = new(2026, 10, 5);
    private static ScheduleTaskInput AllDay(string title = "Hoàn thành bài tập", DateOnly? date = null) =>
        new(title, null, date ?? Oct5, null, null, true, ScheduleTaskPriority.Normal);
    private static ScheduleTaskInput Timed() =>
        new("Ôn kỹ năng giao tiếp", "Chuẩn bị bài thuyết trình", Oct5, new TimeOnly(9, 0), new TimeOnly(10, 30), false, ScheduleTaskPriority.High);

    [Fact]
    public async Task Authenticated_user_can_create_read_update_complete_restore_delete_and_reopen()
    {
        var factory = new TestFactory(); var owner = new CurrentUser { Id = 1 }; var service = new ScheduleService(factory, owner);
        var created = await service.CreateAsync(Timed());
        Assert.Equal("Ôn kỹ năng giao tiếp", created.Title);
        Assert.Equal(created, await service.GetAsync(created.Id));
        Assert.Single(await new ScheduleService(factory, owner).GetPeriodAsync(Oct5, Oct5));
        Assert.True(await service.UpdateAsync(created.Id, AllDay()));
        var updated = await service.GetAsync(created.Id);
        Assert.True(updated!.IsAllDay); Assert.Null(updated.StartTime); Assert.Equal("Hoàn thành bài tập", updated.Title);
        Assert.True(await service.SetCompletedAsync(created.Id, true));
        Assert.Equal(ScheduleTaskStatus.Completed, (await service.GetAsync(created.Id))!.Status);
        Assert.True(await service.SetCompletedAsync(created.Id, false));
        Assert.Equal(ScheduleTaskStatus.Pending, (await service.GetAsync(created.Id))!.Status);
        Assert.True(await service.DeleteAsync(created.Id));
        Assert.Null(await service.GetAsync(created.Id));
    }

    [Fact]
    public async Task Anonymous_user_is_denied_for_every_operation()
    {
        var service = new ScheduleService(new TestFactory(), new CurrentUser());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GetPeriodAsync(Oct5, Oct5));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GetAsync(1));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.CreateAsync(AllDay()));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.UpdateAsync(1, AllDay()));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.SetCompletedAsync(1, true));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.DeleteAsync(1));
    }

    [Fact]
    public async Task Other_user_cannot_read_update_complete_or_delete_another_users_task()
    {
        var factory = new TestFactory(); var current = new CurrentUser { Id = 1 }; var service = new ScheduleService(factory, current);
        var created = await service.CreateAsync(Timed());
        current.Id = 2;
        Assert.Empty(await service.GetPeriodAsync(Oct5, Oct5));
        Assert.Null(await service.GetAsync(created.Id));
        Assert.False(await service.UpdateAsync(created.Id, AllDay()));
        Assert.False(await service.SetCompletedAsync(created.Id, true));
        Assert.False(await service.DeleteAsync(created.Id));
        current.Id = 1;
        Assert.Equal("Ôn kỹ năng giao tiếp", (await service.GetAsync(created.Id))!.Title);
        Assert.Equal(ScheduleTaskStatus.Pending, (await service.GetAsync(created.Id))!.Status);
    }

    [Fact]
    public async Task Queries_only_visible_dates_and_preserve_vietnamese_text()
    {
        var service = new ScheduleService(new TestFactory(), new CurrentUser { Id = 1 });
        await service.CreateAsync(AllDay("Học kỹ năng làm việc nhóm", Oct5));
        await service.CreateAsync(AllDay("Hoàn thành bài tập", Oct5.AddDays(30)));
        var result = await service.GetPeriodAsync(Oct5, Oct5.AddDays(6));
        Assert.Equal("Học kỹ năng làm việc nhóm", Assert.Single(result).Title);
        await Assert.ThrowsAsync<ArgumentException>(() => service.GetPeriodAsync(Oct5, Oct5.AddDays(63)));
    }

    [Fact]
    public void Time_and_required_field_rules_are_enforced()
    {
        ScheduleService.Validate(AllDay());
        ScheduleService.Validate(Timed());
        Assert.Throws<ArgumentException>(() => ScheduleService.Validate(AllDay(" ")));
        Assert.Throws<ArgumentException>(() => ScheduleService.Validate(AllDay(date: default(DateOnly))));
        Assert.Throws<ArgumentException>(() => ScheduleService.Validate(Timed() with { StartTime = null }));
        Assert.Throws<ArgumentException>(() => ScheduleService.Validate(Timed() with { EndTime = new TimeOnly(9, 0) }));
        Assert.Throws<ArgumentException>(() => ScheduleService.Validate(Timed() with { EndTime = new TimeOnly(8, 0) }));
    }

    private sealed class CurrentUser : ICurrentScheduleUser
    {
        public int Id { get; set; }
        public Task<int> GetUserIdAsync() => Task.FromResult(Id);
    }
    private sealed class TestFactory : IDbContextFactory<SoftSyncDbContext>
    {
        private readonly DbContextOptions<SoftSyncDbContext> options = new DbContextOptionsBuilder<SoftSyncDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        public SoftSyncDbContext CreateDbContext() => new(options);
    }
}
