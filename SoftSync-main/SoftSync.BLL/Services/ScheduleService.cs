using Microsoft.EntityFrameworkCore;
using SoftSync.BLL.Interfaces;
using SoftSync.DAL.Data;
using SoftSync.DAL.Entities;

namespace SoftSync.BLL.Services;

public sealed class ScheduleService(IDbContextFactory<SoftSyncDbContext> factory, ICurrentScheduleUser currentUser) : IScheduleService
{
    private async Task<int> RequireUserAsync()
    {
        var id = await currentUser.GetUserIdAsync();
        return id > 0 ? id : throw new UnauthorizedAccessException("Sign in to manage your schedule.");
    }

    public async Task<IReadOnlyList<ScheduleTaskView>> GetPeriodAsync(DateOnly from, DateOnly through, CancellationToken cancellationToken = default)
    {
        var userId = await RequireUserAsync();
        if (through < from || through.DayNumber - from.DayNumber > 62)
            throw new ArgumentException("Schedule period must be between 1 and 63 days.");
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        var items = await db.ScheduleTasks.AsNoTracking()
            .Where(x => x.UserId == userId && x.Date >= from && x.Date <= through)
            .OrderBy(x => x.Date).ThenBy(x => x.IsAllDay ? 0 : 1).ThenBy(x => x.StartTime)
            .ToListAsync(cancellationToken);
        return items.Select(Map).ToList();
    }

    public async Task<ScheduleTaskView?> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var userId = await RequireUserAsync();
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        var task = await db.ScheduleTasks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);
        return task is null ? null : Map(task);
    }

    public async Task<ScheduleTaskView> CreateAsync(ScheduleTaskInput input, CancellationToken cancellationToken = default)
    {
        var userId = await RequireUserAsync();
        Validate(input);
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        var task = new ScheduleTask { UserId = userId };
        Apply(task, input);
        db.ScheduleTasks.Add(task);
        await db.SaveChangesAsync(cancellationToken);
        return Map(task);
    }

    public async Task<bool> UpdateAsync(int id, ScheduleTaskInput input, CancellationToken cancellationToken = default)
    {
        var userId = await RequireUserAsync();
        Validate(input);
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        var task = await db.ScheduleTasks.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);
        if (task is null) return false;
        Apply(task, input);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> SetCompletedAsync(int id, bool completed, CancellationToken cancellationToken = default)
    {
        var userId = await RequireUserAsync();
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        var task = await db.ScheduleTasks.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);
        if (task is null) return false;
        task.Status = completed ? ScheduleTaskStatus.Completed : ScheduleTaskStatus.Pending;
        task.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var userId = await RequireUserAsync();
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        var task = await db.ScheduleTasks.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);
        if (task is null) return false;
        db.ScheduleTasks.Remove(task);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public static void Validate(ScheduleTaskInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Title) || input.Title.Trim().Length > 200)
            throw new ArgumentException("Title is required (maximum 200 characters).", nameof(input));
        if (input.Description?.Length > 2000)
            throw new ArgumentException("Description must be at most 2000 characters.", nameof(input));
        if (input.Date == default)
            throw new ArgumentException("Date is required.", nameof(input));
        if (!Enum.IsDefined(input.Priority))
            throw new ArgumentException("Invalid priority.", nameof(input));
        if (!input.IsAllDay && (input.StartTime is null || input.EndTime is null || input.EndTime <= input.StartTime))
            throw new ArgumentException("A timed task needs a start time and a later end time.", nameof(input));
    }

    private static void Apply(ScheduleTask task, ScheduleTaskInput input)
    {
        task.Title = input.Title.Trim();
        task.Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim();
        task.Date = input.Date;
        task.IsAllDay = input.IsAllDay;
        task.StartTime = input.IsAllDay ? null : input.StartTime;
        task.EndTime = input.IsAllDay ? null : input.EndTime;
        task.Priority = input.Priority;
        task.UpdatedAtUtc = DateTime.UtcNow;
    }

    private static ScheduleTaskView Map(ScheduleTask task) => new(
        task.Id, task.Title, task.Description, task.Date, task.StartTime, task.EndTime,
        task.IsAllDay, task.Priority, task.Status);
}
