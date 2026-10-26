using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SoftSync.DAL.Data;
using SoftSync.DAL.Entities;

namespace SoftSync.Presentation.Services;

public sealed record SystemOverview(long RequestsLastMinute, long ErrorsLastMinute, double? CpuPercent,
    long? WorkingSetBytes, DateTime? CollectedAtUtc, int ActiveLearners, int Evaluations,
    int CvReviews, int AiRequests, string MetricSource);

public sealed class SystemTelemetry : BackgroundService
{
    private sealed class MinuteBucket
    {
        public long Requests, Errors, DurationTicks;
        public long[] LatencyBuckets { get; } = new long[7];
        public readonly object Gate = new();
        public double CpuTotal, PeakCpu;
        public long PeakWorkingSet;
        public int CpuSamples;
        public DateTime LastSampleUtc;
    }

    private readonly ConcurrentDictionary<DateTime, MinuteBucket> buckets = new();
    private readonly IServiceScopeFactory scopeFactory;
    private readonly ILogger<SystemTelemetry> logger;
    private readonly Process process = Process.GetCurrentProcess();
    private readonly string instanceId = $"{Environment.MachineName}-{Environment.ProcessId}";
    private DateTime lastCpuAt = DateTime.UtcNow;
    private TimeSpan lastCpuTime = Process.GetCurrentProcess().TotalProcessorTime;
    private DateTime lastRetentionAtUtc = DateTime.MinValue;

    public SystemTelemetry(IServiceScopeFactory scopeFactory, ILogger<SystemTelemetry> logger)
    { this.scopeFactory = scopeFactory; this.logger = logger; }

    public void RecordRequest(double durationMs, int statusCode)
    {
        var minute = new DateTime(DateTime.UtcNow.Ticks / TimeSpan.TicksPerMinute * TimeSpan.TicksPerMinute, DateTimeKind.Utc);
        var bucket = buckets.GetOrAdd(minute, _ => new MinuteBucket());
        Interlocked.Increment(ref bucket.Requests);
        if (statusCode >= 500) Interlocked.Increment(ref bucket.Errors);
        Interlocked.Add(ref bucket.DurationTicks, (long)(durationMs * 1000));
        var bound = durationMs <= 100 ? 0 : durationMs <= 250 ? 1 : durationMs <= 500 ? 2 : durationMs <= 1000 ? 3 : durationMs <= 3000 ? 4 : durationMs <= 10000 ? 5 : 6;
        Interlocked.Increment(ref bucket.LatencyBuckets[bound]);
        if (buckets.Count > 120)
            foreach (var stale in buckets.Keys.OrderBy(x => x).Take(buckets.Count - 120)) buckets.TryRemove(stale, out _);
    }

    public (long Requests, long Errors, double? Cpu, long? Memory, DateTime? SampleAt) GetLive()
    {
        var minute = new DateTime(DateTime.UtcNow.Ticks / TimeSpan.TicksPerMinute * TimeSpan.TicksPerMinute, DateTimeKind.Utc);
        var bucket = buckets.GetOrAdd(minute, _ => new MinuteBucket());
        lock (bucket.Gate) return (Interlocked.Read(ref bucket.Requests), Interlocked.Read(ref bucket.Errors),
            bucket.CpuSamples == 0 ? null : bucket.CpuTotal / bucket.CpuSamples, bucket.PeakWorkingSet == 0 ? null : bucket.PeakWorkingSet, bucket.LastSampleUtc == default ? null : bucket.LastSampleUtc);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(15));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                SampleProcess();
                await FlushAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
    }

    private void SampleProcess()
    {
        try
        {
            process.Refresh();
            var now = DateTime.UtcNow; var cpuNow = process.TotalProcessorTime;
            var elapsed = (now - lastCpuAt).TotalMilliseconds;
            var cpu = elapsed <= 0 ? 0 : Math.Clamp((cpuNow - lastCpuTime).TotalMilliseconds / elapsed / Math.Max(1, Environment.ProcessorCount) * 100, 0, 100);
            lastCpuAt = now; lastCpuTime = cpuNow;
            var minute = new DateTime(now.Ticks / TimeSpan.TicksPerMinute * TimeSpan.TicksPerMinute, DateTimeKind.Utc);
            var bucket = buckets.GetOrAdd(minute, _ => new MinuteBucket());
            lock (bucket.Gate)
            {
                bucket.CpuTotal += cpu; bucket.CpuSamples++; bucket.PeakCpu = Math.Max(bucket.PeakCpu, cpu);
                bucket.PeakWorkingSet = Math.Max(bucket.PeakWorkingSet, process.WorkingSet64); bucket.LastSampleUtc = now;
            }
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception)
        { logger.LogDebug("Could not sample current process resources: {Type}", ex.GetType().Name); }
    }

    private async Task FlushAsync(CancellationToken cancellationToken)
    {
        var cutoff = new DateTime(DateTime.UtcNow.Ticks / TimeSpan.TicksPerMinute * TimeSpan.TicksPerMinute, DateTimeKind.Utc);
        var ready = buckets.Where(x => x.Key < cutoff).ToArray();
        if (ready.Length == 0) return;
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SoftSyncDbContext>();
            foreach (var (minute, bucket) in ready)
            {
                var samples = Math.Max(1, bucket.CpuSamples);
                var row = await db.SystemMetricMinutes.SingleOrDefaultAsync(x => x.MinuteStartUtc == minute && x.InstanceId == instanceId, cancellationToken);
                if (row is null)
                {
                    row = new SystemMetricMinute { MinuteStartUtc = minute, InstanceId = instanceId };
                    db.SystemMetricMinutes.Add(row);
                }
                row.AverageCpuPercent = bucket.CpuSamples == 0 ? null : bucket.CpuTotal / bucket.CpuSamples;
                row.PeakCpuPercent = bucket.CpuSamples == 0 ? null : bucket.PeakCpu;
                row.PeakWorkingSetBytes = bucket.PeakWorkingSet == 0 ? null : bucket.PeakWorkingSet;
                row.CpuSampleCount = bucket.CpuSamples;
                row.RequestCount = Interlocked.Read(ref bucket.Requests); row.ErrorCount = Interlocked.Read(ref bucket.Errors);
                row.RequestDurationTotalMs = Interlocked.Read(ref bucket.DurationTicks) / 1000d;
                row.LatencyBucketsJson = JsonSerializer.Serialize(bucket.LatencyBuckets.Select(x => x).ToArray());
                await db.SaveChangesAsync(cancellationToken);
                buckets.TryRemove(minute, out _);
            }
            if (DateTime.UtcNow - lastRetentionAtUtc >= TimeSpan.FromHours(24))
            {
                var metricRetentionCutoff = DateTime.UtcNow.AddDays(-30);
                var expiredMetrics = await db.SystemMetricMinutes.Where(x => x.MinuteStartUtc < metricRetentionCutoff).ToListAsync(cancellationToken);
                db.SystemMetricMinutes.RemoveRange(expiredMetrics);
                var heartbeatCutoff = DateTime.UtcNow.AddDays(-90);
                var expiredHeartbeats = await db.LearningHeartbeats.Where(x => x.AcceptedAtUtc < heartbeatCutoff).ToListAsync(cancellationToken);
                db.LearningHeartbeats.RemoveRange(expiredHeartbeats);
                await db.SaveChangesAsync(cancellationToken);
                lastRetentionAtUtc = DateTime.UtcNow;
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        { logger.LogWarning("System metrics flush failed; retaining bounded in-process samples: {Type}", ex.GetType().Name); }
    }

    public override void Dispose() { process.Dispose(); base.Dispose(); }
}

public sealed class SystemMetricsMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, SystemTelemetry telemetry)
    {
        var path = context.Request.Path;
        if (path == "/health" || path.StartsWithSegments("/_blazor") || path.StartsWithSegments("/css") ||
            path.StartsWithSegments("/js") || path.StartsWithSegments("/images") || path.StartsWithSegments("/fonts") ||
            path.StartsWithSegments("/bootstrap") || path.StartsWithSegments("/api/admin/telemetry"))
        { await next(context); return; }
        var started = Stopwatch.GetTimestamp();
        try { await next(context); }
        finally { telemetry.RecordRequest(Stopwatch.GetElapsedTime(started).TotalMilliseconds, context.Response.StatusCode); }
    }
}

public sealed class AdminAnalyticsService(SoftSyncDbContext db, SystemTelemetry telemetry)
{
    public sealed record LearnerQualityRow(int UserId, string Name, string Email, int MeasuredSeconds,
        int LearningSessions, double? QuizAverage, double? AssessmentAverage, int CvReviews);
    public async Task<SystemOverview> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        var live = telemetry.GetLive();
        var day = DateTime.UtcNow.AddDays(-1);
        var active = await db.LearningActivitySessions.AsNoTracking().Where(x => x.LastHeartbeatAtUtc >= day && x.MeasuredSeconds > 0)
            .Select(x => x.UserId).Distinct().CountAsync(cancellationToken);
        var evaluations = await db.AssessmentResults.AsNoTracking().CountAsync(x => x.CreatedAt >= day, cancellationToken);
        var reviews = await db.CvReviews.AsNoTracking().CountAsync(x => x.CreatedAtUtc >= day, cancellationToken);
        var ai = await db.AiOperations.AsNoTracking().CountAsync(x => x.CreatedAtUtc >= day, cancellationToken);
        return new(live.Requests, live.Errors, live.Cpu, live.Memory, live.SampleAt, active, evaluations, reviews, ai,
            $"Process {Environment.ProcessId}; CPU tính trên {Environment.ProcessorCount} logical CPU.");
    }

    public Task<List<SystemMetricMinute>> GetHistoryAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default) =>
        db.SystemMetricMinutes.AsNoTracking().Where(x => x.MinuteStartUtc >= fromUtc && x.MinuteStartUtc < toUtc)
            .OrderBy(x => x.MinuteStartUtc).Take(10_000).ToListAsync(cancellationToken);

    public Task<int> GetLearnerCountAsync(CancellationToken cancellationToken = default) =>
        (from user in db.Users
         where !db.UserRoles.Join(db.Roles, membership => membership.RoleId, role => role.Id, (membership, role) => new { membership.UserId, role.Name })
             .Any(x => x.UserId == user.Id && (x.Name == "Admin" || x.Name == "Teacher"))
         select user.Id).CountAsync(cancellationToken);

    public async Task<List<LearnerQualityRow>> GetLearnerQualityAsync(CancellationToken cancellationToken = default)
    {
        var learners = await (from user in db.Users
            where !db.UserRoles.Join(db.Roles, membership => membership.RoleId, role => role.Id,
                (membership, role) => new { membership.UserId, role.Name })
                .Any(x => x.UserId == user.Id && (x.Name == "Admin" || x.Name == "Teacher"))
            select new { user.Id, user.FullName, user.DisplayName, user.Email }).ToListAsync(cancellationToken);
        var times = await db.LearningActivitySessions.AsNoTracking().GroupBy(x => x.UserId)
            .Select(g => new { UserId = g.Key, Seconds = g.Sum(x => x.MeasuredSeconds), Sessions = g.Count() }).ToListAsync(cancellationToken);
        var quizzes = await db.QuizAttempts.AsNoTracking().Where(x => x.CompletedAtUtc != null).GroupBy(x => x.UserId)
            .Select(g => new { UserId = g.Key, Average = g.Average(x => x.ScorePercentage) }).ToListAsync(cancellationToken);
        var assessments = await db.AssessmentResults.AsNoTracking().GroupBy(x => x.UserId)
            .Select(g => new { UserId = g.Key, Average = g.Average(x => x.Score) }).ToListAsync(cancellationToken);
        var cvs = await db.CvReviews.AsNoTracking().GroupBy(x => x.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() }).ToListAsync(cancellationToken);
        var timeByUser = times.ToDictionary(x => x.UserId); var quizByUser = quizzes.ToDictionary(x => x.UserId);
        var assessmentByUser = assessments.ToDictionary(x => x.UserId); var cvByUser = cvs.ToDictionary(x => x.UserId);
        return learners.Select(user => new LearnerQualityRow(user.Id,
            string.IsNullOrWhiteSpace(user.DisplayName) ? user.FullName : user.DisplayName, user.Email ?? "",
            timeByUser.GetValueOrDefault(user.Id)?.Seconds ?? 0, timeByUser.GetValueOrDefault(user.Id)?.Sessions ?? 0,
            quizByUser.TryGetValue(user.Id, out var quiz) ? (double?)Convert.ToDouble(quiz.Average) : null,
            assessmentByUser.TryGetValue(user.Id, out var assessment) ? (double?)assessment.Average : null,
            cvByUser.GetValueOrDefault(user.Id)?.Count ?? 0))
            .OrderByDescending(x => x.MeasuredSeconds).ThenBy(x => x.Name).ToList();
    }
}
