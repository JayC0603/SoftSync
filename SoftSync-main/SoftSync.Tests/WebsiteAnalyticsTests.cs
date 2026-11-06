using Microsoft.EntityFrameworkCore;
using SoftSync.BLL.Services;
using SoftSync.DAL.Data;
using Xunit;

namespace SoftSync.Tests;

public sealed class WebsiteAnalyticsTests
{
    private sealed class Clock(DateTime now) : TimeProvider
    {
        public DateTime Now = now;
        public override DateTimeOffset GetUtcNow() => new(DateTime.SpecifyKind(Now, DateTimeKind.Utc));
        public void Advance(int seconds) => Now = Now.AddSeconds(seconds);
    }
    private static SoftSyncDbContext Database() => new(new DbContextOptionsBuilder<SoftSyncDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Theory]
    [InlineData(VisitPeriod.Day, "2026-10-26", "2026-10-26", "2026-10-27")]
    [InlineData(VisitPeriod.Week, "2026-11-01", "2026-10-26", "2026-11-02")]
    [InlineData(VisitPeriod.Month, "2028-02-29", "2028-02-01", "2028-03-01")]
    public void Calendar_ranges_use_Monday_and_actual_month_lengths(VisitPeriod period, string date, string start, string end)
    {
        var range = WebsiteAnalyticsService.LocalRange(period, DateTime.Parse(date));
        Assert.Equal(DateTime.Parse(start), range.Start);
        Assert.Equal(DateTime.Parse(end), range.End);
    }

    [Fact]
    public async Task Repeated_heartbeats_and_end_are_idempotent_and_inactive_time_is_excluded()
    {
        await using var db = Database(); var clock = new Clock(new(2026, 10, 26, 3, 0, 0));
        var service = new WebsiteAnalyticsService(db, clock);
        var id = await service.StartAsync(true);
        clock.Advance(20); await service.UpdateAsync(id, 1, false, false);
        await service.UpdateAsync(id, 1, true, false);
        clock.Advance(20); await service.UpdateAsync(id, 2, true, false);
        clock.Advance(10); await service.UpdateAsync(id, 3, false, true);
        await service.UpdateAsync(id, 4, false, true);
        var report = await service.GetReportAsync(VisitPeriod.Day, new(2026, 10, 26));
        Assert.Equal(1, report.Entries); Assert.Equal(1, report.Exits); Assert.Equal(0, report.Online);
        Assert.Equal(30, report.ActiveSeconds); Assert.Equal(24, report.Buckets.Count);
    }

    [Fact]
    public async Task Activity_is_split_at_Vietnam_midnight_and_exit_belongs_to_exit_date()
    {
        await using var db = Database(); var clock = new Clock(new(2026, 10, 26, 16, 59, 50));
        var service = new WebsiteAnalyticsService(db, clock); var id = await service.StartAsync(true);
        clock.Advance(20); await service.UpdateAsync(id, 1, false, true);
        var before = await service.GetReportAsync(VisitPeriod.Day, new(2026, 10, 26));
        var after = await service.GetReportAsync(VisitPeriod.Day, new(2026, 10, 27));
        Assert.Equal(1, before.Entries); Assert.Equal(0, before.Exits); Assert.Equal(10, before.ActiveSeconds);
        Assert.Equal(0, after.Entries); Assert.Equal(1, after.Exits); Assert.Equal(10, after.ActiveSeconds);
    }

    [Fact]
    public async Task Timeout_counts_one_exit_and_never_credits_a_disconnected_gap()
    {
        await using var db = Database(); var clock = new Clock(new(2026, 10, 26, 3, 0, 0));
        var service = new WebsiteAnalyticsService(db, clock); var id = await service.StartAsync(true);
        clock.Advance(20); await service.UpdateAsync(id, 1, true, false);
        clock.Advance(90); Assert.False(await service.UpdateAsync(id, 2, true, false));
        var report = await service.GetReportAsync(VisitPeriod.Week, new(2026, 10, 26));
        Assert.Equal(1, report.Exits); Assert.Equal(0, report.Online); Assert.Equal(20, report.ActiveSeconds);
        Assert.Equal(7, report.Buckets.Count);
    }

    [Fact]
    public async Task Empty_month_contains_zero_filled_calendar_buckets()
    {
        await using var db = Database(); var clock = new Clock(new(2026, 10, 26));
        var report = await new WebsiteAnalyticsService(db, clock).GetReportAsync(VisitPeriod.Month, new(2026, 9, 10));
        Assert.Equal(30, report.Buckets.Count);
        Assert.All(report.Buckets, x => { Assert.Equal(0, x.Entries); Assert.Equal(0, x.Exits); Assert.Equal(0, x.ActiveSeconds); });
    }
}
