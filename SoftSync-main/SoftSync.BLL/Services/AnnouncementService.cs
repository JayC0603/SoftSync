using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SoftSync.DAL.Data;
using SoftSync.DAL.Entities;

namespace SoftSync.BLL.Services;

public sealed record NotificationItem(int Id, string Title, string Body, DateTime PublishedAtUtc, bool IsRead);
public sealed record NotificationPage(IReadOnlyList<NotificationItem> Items, int Total, int Unread);
public sealed record PublishedAnnouncement(int Id, string Title, string Body, DateTime PublishedAtUtc, int Recipients);

/// <summary>Local circuits refresh immediately; polling also covers other app instances.</summary>
public sealed class NotificationChanges(ILogger<NotificationChanges> logger)
{
    public event Action<int?>? Changed;
    public void Notify(int? userId = null)
    {
        if (Changed is not { } handlers) return;
        foreach (Action<int?> handler in handlers.GetInvocationList())
        {
            try { handler(userId); }
            catch (Exception ex) { logger.LogWarning(ex, "A notification subscriber could not refresh"); }
        }
    }
}

public sealed class AnnouncementService(SoftSyncDbContext db, NotificationChanges changes)
{
    public async Task<NotificationPage> GetInboxAsync(ClaimsPrincipal caller, int page = 1, int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = UserId(caller);
        var query = db.UserNotifications.AsNoTracking().Where(x => x.UserId == userId);
        var total = await query.CountAsync(cancellationToken);
        var unread = await query.CountAsync(x => x.ReadAtUtc == null, cancellationToken);
        pageSize = Math.Clamp(pageSize, 1, 50);
        page = Math.Clamp(page, 1, Math.Max(1, (int)Math.Ceiling(total / (double)pageSize)));
        var items = await query.OrderByDescending(x => x.Announcement.PublishedAtUtc)
            .ThenByDescending(x => x.AnnouncementId).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new NotificationItem(x.AnnouncementId, x.Announcement.Title, x.Announcement.Body,
                x.Announcement.PublishedAtUtc, x.ReadAtUtc != null)).ToListAsync(cancellationToken);
        return new(items, total, unread);
    }

    public async Task MarkReadAsync(ClaimsPrincipal caller, int? announcementId = null)
    {
        var userId = UserId(caller);
        var query = db.UserNotifications.Where(x => x.UserId == userId && x.ReadAtUtc == null);
        if (announcementId.HasValue) query = query.Where(x => x.AnnouncementId == announcementId.Value);
        var notifications = await query.ToListAsync();
        if (notifications.Count == 0) return;
        var now = DateTime.UtcNow;
        foreach (var notification in notifications) notification.ReadAtUtc = now;
        await db.SaveChangesAsync();
        changes.Notify(userId);
    }

    public async Task<PublishedAnnouncement> PublishAsync(ClaimsPrincipal caller, string title, string body)
    {
        var senderId = await RequireAdminAsync(caller);
        title = title?.Trim() ?? string.Empty;
        body = body?.Trim() ?? string.Empty;
        if (title.Length is < 1 or > 160 || body.Length is < 1 or > 4000)
            throw new ArgumentException("A title (1–160 characters) and message (1–4000 characters) are required.");
        var userIds = await db.Users.AsNoTracking().Select(x => x.Id).ToListAsync();
        var announcement = new Announcement { Title = title, Body = body, SenderUserId = senderId };
        foreach (var id in userIds) announcement.Recipients.Add(new UserNotification { UserId = id });
        // One SaveChanges transaction persists the announcement and every recipient.
        db.Announcements.Add(announcement);
        await db.SaveChangesAsync();
        changes.Notify();
        return new(announcement.Id, title, body, announcement.PublishedAtUtc, userIds.Count);
    }

    public async Task<IReadOnlyList<PublishedAnnouncement>> GetPublishedAsync(ClaimsPrincipal caller)
    {
        await RequireAdminAsync(caller);
        return await db.Announcements.AsNoTracking().OrderByDescending(x => x.PublishedAtUtc)
            .ThenByDescending(x => x.Id).Take(50)
            .Select(x => new PublishedAnnouncement(x.Id, x.Title, x.Body, x.PublishedAtUtc, x.Recipients.Count))
            .ToListAsync();
    }

    private async Task<int> RequireAdminAsync(ClaimsPrincipal caller)
    {
        var id = UserId(caller);
        // Recheck Identity membership so a revoked Admin session cannot publish.
        if (!caller.IsInRole("Admin") || !await (from membership in db.UserRoles
                join role in db.Roles on membership.RoleId equals role.Id
                where membership.UserId == id && role.NormalizedName == "ADMIN"
                select membership).AnyAsync())
            throw new UnauthorizedAccessException();
        return id;
    }

    private static int UserId(ClaimsPrincipal caller)
    {
        if (caller.Identity?.IsAuthenticated != true ||
            !int.TryParse(caller.FindFirstValue(ClaimTypes.NameIdentifier), out var id) || id <= 0)
            throw new UnauthorizedAccessException();
        return id;
    }
}
