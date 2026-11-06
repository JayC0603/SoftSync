using Microsoft.EntityFrameworkCore;
using SoftSync.DAL.Data;
using SoftSync.DAL.Entities;

namespace SoftSync.BLL.Services;

public enum VisitPeriod { Day, Week, Month }
public sealed record VisitBucket(DateTime LocalStart, int Entries, int Exits, double ActiveSeconds);
public sealed record VisitReport(DateTime LocalStart, DateTime LocalEnd, int Entries, int Exits,
    int Online, double ActiveSeconds, IReadOnlyList<VisitBucket> Buckets);

public sealed class WebsiteAnalyticsService(SoftSyncDbContext db, TimeProvider clock)
{
    public static readonly TimeSpan VietnamOffset = TimeSpan.FromHours(7);
    public static readonly TimeSpan VisitTimeout = TimeSpan.FromSeconds(90);
    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public async Task<Guid> StartAsync(bool active, CancellationToken ct = default)
    {
        var now = Now;
        var visit = new WebsiteVisit { Id = Guid.NewGuid(), StartedAtUtc = now, LastSeenAtUtc = now, IsActive = active };
        db.WebsiteVisits.Add(visit);
        await db.SaveChangesAsync(ct);
        return visit.Id;
    }

    public async Task<bool> UpdateAsync(Guid id, int sequence, bool active, bool end, CancellationToken ct = default)
    {
        var visit = await db.WebsiteVisits.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (visit is null || visit.EndedAtUtc != null) return false;
        if (sequence <= visit.Sequence) return true;
        var now = Now;
        var gap = now - visit.LastSeenAtUtc;
        // A suspended/disconnected tab must not turn a long gap into activity.
        if (gap >= VisitTimeout) return false;
        if (visit.IsActive && gap > TimeSpan.Zero && gap <= TimeSpan.FromSeconds(45))
        {
            for (var from = visit.LastSeenAtUtc; from < now;)
            {
                var minute = new DateTime(from.Ticks / TimeSpan.TicksPerMinute * TimeSpan.TicksPerMinute, DateTimeKind.Utc);
                var to = minute.AddMinutes(1) < now ? minute.AddMinutes(1) : now;
                var row = await db.WebsiteActivityMinutes.FindAsync([id, minute], ct);
                if (row is null) { row = new() { VisitId = id, MinuteUtc = minute }; db.WebsiteActivityMinutes.Add(row); }
                row.ActiveSeconds += (to - from).TotalSeconds;
                from = to;
            }
        }
        visit.LastSeenAtUtc = now;
        visit.Sequence = sequence;
        visit.IsActive = active && !end;
        if (end) visit.EndedAtUtc = now;
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { db.ChangeTracker.Clear(); } // A newer heartbeat won; do not double-count.
        return true;
    }

    public static (DateTime Start, DateTime End) LocalRange(VisitPeriod period, DateTime anchor)
    {
        var day = DateTime.SpecifyKind(anchor.Date, DateTimeKind.Unspecified);
        var start = period switch
        {
            VisitPeriod.Week => day.AddDays(-((int)day.DayOfWeek + 6) % 7),
            VisitPeriod.Month => new DateTime(day.Year, day.Month, 1),
            _ => day
        };
        return (start, period switch { VisitPeriod.Week => start.AddDays(7), VisitPeriod.Month => start.AddMonths(1), _ => start.AddDays(1) });
    }

    public async Task<VisitReport> GetReportAsync(VisitPeriod period, DateTime anchor, CancellationToken ct = default)
    {
        var (start, end) = LocalRange(period, anchor);
        var fromUtc = DateTime.SpecifyKind(start - VietnamOffset, DateTimeKind.Utc);
        var toUtc = DateTime.SpecifyKind(end - VietnamOffset, DateTimeKind.Utc);
        var now = Now;
        var cutoff = now - VisitTimeout;
        var overlapCutoff = fromUtc - VisitTimeout;
        var visits = await db.WebsiteVisits.AsNoTracking()
            .Where(x => x.StartedAtUtc < toUtc && (x.StartedAtUtc >= fromUtc || x.LastSeenAtUtc >= overlapCutoff))
            .ToListAsync(ct);
        // Aggregate in PostgreSQL before materializing; one row per minute, not per visitor.
        var activity = await db.WebsiteActivityMinutes.AsNoTracking()
            .Where(x => x.MinuteUtc >= fromUtc && x.MinuteUtc < toUtc)
            .GroupBy(x => x.MinuteUtc).Select(g => new { Minute = g.Key, Seconds = g.Sum(x => x.ActiveSeconds) }).ToListAsync(ct);
        var online = await db.WebsiteVisits.AsNoTracking().CountAsync(x => x.EndedAtUtc == null && x.LastSeenAtUtc > cutoff, ct);
        var buckets = new List<VisitBucket>();
        var step = period == VisitPeriod.Day ? TimeSpan.FromHours(1) : TimeSpan.FromDays(1);
        for (var local = start; local < end; local += step)
        {
            var a = DateTime.SpecifyKind(local - VietnamOffset, DateTimeKind.Utc);
            var b = a + step;
            var entries = visits.Count(x => x.StartedAtUtc >= a && x.StartedAtUtc < b && x.StartedAtUtc <= now);
            var exits = visits.Count(x => { var exit = x.EndedAtUtc ?? x.LastSeenAtUtc + VisitTimeout; return exit <= now && exit >= a && exit < b; });
            buckets.Add(new(local, entries, exits, activity.Where(x => x.Minute >= a && x.Minute < b).Sum(x => x.Seconds)));
        }
        return new(start, end, buckets.Sum(x => x.Entries), buckets.Sum(x => x.Exits), online,
            buckets.Sum(x => x.ActiveSeconds), buckets);
    }
}
