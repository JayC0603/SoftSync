using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SoftSync.DAL.Data;
using SoftSync.DAL.Entities;

namespace SoftSync.Presentation.Services;

public sealed record StartLearningActivityRequest(string ActivityType, string? ResourceId);
public sealed record LearningActivityStarted(Guid SessionId, DateTime StartedAtUtc);
public sealed record LearningHeartbeatRequest(Guid SessionId, int Sequence);

public sealed class LearningActivityService(SoftSyncDbContext db)
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
        { "lesson", "roadmap", "quiz", "roleplay", "practice", "assessment" };

    public async Task<LearningActivityStarted> StartAsync(int userId, StartLearningActivityRequest request, CancellationToken ct)
    {
        var type = (request.ActivityType ?? "").Trim().ToLowerInvariant();
        if (!AllowedTypes.Contains(type)) throw new ArgumentException("Unsupported activity type.");
        var now = DateTime.UtcNow;
        var session = new LearningActivitySession
        {
            Id = Guid.NewGuid(), UserId = userId, ActivityType = type,
            ResourceId = (request.ResourceId ?? "").Trim()[..Math.Min(120, (request.ResourceId ?? "").Trim().Length)],
            StartedAtUtc = now, LastHeartbeatAtUtc = now
        };
        db.LearningActivitySessions.Add(session);
        await db.SaveChangesAsync(ct);
        return new(session.Id, now);
    }

    public async Task<bool> HeartbeatAsync(int userId, LearningHeartbeatRequest request, CancellationToken ct)
    {
        if (request.Sequence <= 0 || request.Sequence > 2_000_000) return false;
        var session = await db.LearningActivitySessions.SingleOrDefaultAsync(x => x.Id == request.SessionId && x.UserId == userId, ct);
        if (session is null || session.EndedAtUtc.HasValue) return false;
        if (await db.LearningHeartbeats.AnyAsync(x => x.SessionId == request.SessionId && x.Sequence == request.Sequence, ct)) return true;
        var now = DateTime.UtcNow;
        var seconds = (int)Math.Clamp((now - session.LastHeartbeatAtUtc).TotalSeconds, 0, 45);
        db.LearningHeartbeats.Add(new LearningHeartbeat
        {
            SessionId = session.Id, Sequence = request.Sequence, FromUtc = session.LastHeartbeatAtUtc,
            ToUtc = now, AcceptedAtUtc = now, CreditedSeconds = seconds
        });
        session.MeasuredSeconds = checked(session.MeasuredSeconds + seconds);
        session.LastHeartbeatAtUtc = now;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task EndAsync(int userId, Guid sessionId, CancellationToken ct)
    {
        var session = await db.LearningActivitySessions.SingleOrDefaultAsync(x => x.Id == sessionId && x.UserId == userId, ct);
        if (session is null || session.EndedAtUtc.HasValue) return;
        session.EndedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public static bool TryGetUserId(ClaimsPrincipal user, out int id) =>
        int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out id);
}
