using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SoftSync.DAL.Data;
using SoftSync.DAL.Entities;

namespace SoftSync.BLL.Services;

public sealed record MentorRequestView(int Id, int MentorId, string MentorName, string Goal,
    string Skill, string Context, string PreferredContact, string Status, DateTime CreatedAtUtc, string? Email);

public sealed class MentorRequestWorkflow(SoftSyncDbContext db, NotificationChanges changes)
{
    public async Task<IReadOnlyList<MentorRequestView>> ListAsync(ClaimsPrincipal caller, bool manage = false)
    {
        var userId = UserId(caller);
        if (manage) await RequireAdminAsync(caller);
        var requests = db.MentorSupportRequests.AsNoTracking().AsQueryable();
        if (!manage) requests = requests.Where(x => x.UserId == userId);
        return await (from request in requests
                      join mentor in db.Mentors on request.MentorId equals mentor.Id
                      join user in db.Users on request.UserId equals user.Id
                      orderby request.CreatedAtUtc descending, request.Id descending
                      select new MentorRequestView(request.Id, mentor.Id, mentor.Name, request.Goal,
                          request.ShareLearningContext ? request.Skill : "", request.Context,
                          request.PreferredContact, request.Status, request.CreatedAtUtc,
                          manage && request.PreferredContact == "Email" ? user.Email : null)).Take(100).ToListAsync();
    }

    public async Task<bool> RespondAsync(ClaimsPrincipal caller, int requestId, string status, string reply)
    {
        var senderId = await RequireAdminAsync(caller);
        reply = reply?.Trim() ?? "";
        if (status is not ("InProgress" or "Resolved" or "Declined") || reply.Length is < 3 or > 3000)
            throw new ArgumentException("A valid status and a reply of 3–3000 characters are required.");
        var request = await db.MentorSupportRequests.FindAsync(requestId);
        if (request is null || request.Status is "Resolved" or "Declined") return false;
        request.Status = status;
        // Deliver the response privately through the existing persistent inbox.
        // The state change and notification commit in the same transaction.
        var notification = new Announcement
        {
            SenderUserId = senderId,
            Title = $"Mentor · #{request.Id} · {status}",
            Body = $"{request.Goal}\n\n{reply}",
            Recipients = [new UserNotification { UserId = request.UserId }]
        };
        db.Announcements.Add(notification);
        await db.SaveChangesAsync();
        changes.Notify(request.UserId);
        return true;
    }

    private async Task<int> RequireAdminAsync(ClaimsPrincipal caller)
    {
        var userId = UserId(caller);
        if (!caller.IsInRole("Admin") || !await (from membership in db.UserRoles
            join role in db.Roles on membership.RoleId equals role.Id
            where membership.UserId == userId && role.NormalizedName == "ADMIN"
            select membership).AnyAsync()) throw new UnauthorizedAccessException();
        return userId;
    }

    private static int UserId(ClaimsPrincipal caller)
    {
        if (caller.Identity?.IsAuthenticated != true ||
            !int.TryParse(caller.FindFirstValue(ClaimTypes.NameIdentifier), out var id) || id <= 0)
            throw new UnauthorizedAccessException();
        return id;
    }
}
