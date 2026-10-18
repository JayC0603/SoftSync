using System.ComponentModel.DataAnnotations;

namespace SoftSync.DAL.Entities;

public sealed class Announcement
{
    public int Id { get; set; }
    [Required, MaxLength(160)] public string Title { get; set; } = string.Empty;
    [Required, MaxLength(4000)] public string Body { get; set; } = string.Empty;
    public int SenderUserId { get; set; }
    public ApplicationUser Sender { get; set; } = null!;
    public DateTime PublishedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<UserNotification> Recipients { get; set; } = new List<UserNotification>();
}

public sealed class UserNotification
{
    public int AnnouncementId { get; set; }
    public Announcement Announcement { get; set; } = null!;
    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public DateTime? ReadAtUtc { get; set; }
}
