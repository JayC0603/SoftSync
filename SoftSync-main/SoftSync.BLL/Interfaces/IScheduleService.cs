using SoftSync.DAL.Entities;

namespace SoftSync.BLL.Interfaces;

// Implemented by the presentation layer from the server-side authentication state.
public interface ICurrentScheduleUser
{
    Task<int> GetUserIdAsync();
}

public sealed record ScheduleTaskInput(
    string Title, string? Description, DateOnly Date, TimeOnly? StartTime,
    TimeOnly? EndTime, bool IsAllDay, ScheduleTaskPriority Priority);

public sealed record ScheduleTaskView(
    int Id, string Title, string? Description, DateOnly Date, TimeOnly? StartTime,
    TimeOnly? EndTime, bool IsAllDay, ScheduleTaskPriority Priority,
    ScheduleTaskStatus Status);

public interface IScheduleService
{
    Task<IReadOnlyList<ScheduleTaskView>> GetPeriodAsync(DateOnly from, DateOnly through, CancellationToken cancellationToken = default);
    Task<ScheduleTaskView?> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<ScheduleTaskView> CreateAsync(ScheduleTaskInput input, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, ScheduleTaskInput input, CancellationToken cancellationToken = default);
    Task<bool> SetCompletedAsync(int id, bool completed, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
