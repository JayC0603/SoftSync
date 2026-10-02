namespace SoftSync.DAL.Entities;

public enum ScheduleTaskPriority { Low, Normal, High }
public enum ScheduleTaskStatus { Pending, Completed }

public sealed class ScheduleTask
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public bool IsAllDay { get; set; }
    public ScheduleTaskPriority Priority { get; set; } = ScheduleTaskPriority.Normal;
    public ScheduleTaskStatus Status { get; set; } = ScheduleTaskStatus.Pending;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
