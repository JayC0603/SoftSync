using System.ComponentModel.DataAnnotations;

namespace SoftSync.DAL.Entities;

public sealed class WebsiteVisit
{
    public Guid Id { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime LastSeenAtUtc { get; set; }
    public DateTime? EndedAtUtc { get; set; }
    public bool IsActive { get; set; }
    [ConcurrencyCheck] public int Sequence { get; set; }
}

public sealed class WebsiteActivityMinute
{
    public Guid VisitId { get; set; }
    public DateTime MinuteUtc { get; set; }
    public double ActiveSeconds { get; set; }
}
