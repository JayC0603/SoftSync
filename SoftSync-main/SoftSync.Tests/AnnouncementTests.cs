using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using SoftSync.BLL.Services;
using SoftSync.DAL.Data;
using SoftSync.DAL.Entities;
using Xunit;

namespace SoftSync.Tests;

public sealed class AnnouncementTests
{
    private static ClaimsPrincipal Caller(int id, string role = "User") => new(new ClaimsIdentity(
        [new(ClaimTypes.NameIdentifier, id.ToString()), new(ClaimTypes.Role, role)], "test"));
    private static NotificationChanges Changes() => new(NullLogger<NotificationChanges>.Instance);
    private static async Task<SoftSyncDbContext> DatabaseAsync()
    {
        var db = new SoftSyncDbContext(new DbContextOptionsBuilder<SoftSyncDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        await db.Database.EnsureCreatedAsync();
        db.Users.AddRange(new ApplicationUser { Id = 101, UserName = "admin" },
            new ApplicationUser { Id = 102, UserName = "alice" }, new ApplicationUser { Id = 103, UserName = "bob" });
        db.Roles.Add(new IdentityRole<int> { Id = 101, Name = "Admin", NormalizedName = "ADMIN" });
        db.UserRoles.Add(new IdentityUserRole<int> { UserId = 101, RoleId = 101 });
        await db.SaveChangesAsync();
        return db;
    }

    [Fact]
    public async Task Broadcast_persists_for_all_existing_accounts_and_reads_are_private_and_idempotent()
    {
        await using var db = await DatabaseAsync();
        var service = new AnnouncementService(db, Changes());
        var sent = await service.PublishAsync(Caller(101, "Admin"), "  Bảo trì  ", "  Nội dung thông báo\nDòng thứ hai  ");
        Assert.Equal(3, sent.Recipients);
        Assert.Equal("Bảo trì", sent.Title);
        Assert.Equal(3, await db.UserNotifications.CountAsync());
        // Fresh context represents a later login; delivery doesn't depend on a live circuit.
        await using var later = new SoftSyncDbContext((DbContextOptions<SoftSyncDbContext>)db.GetService<IDbContextOptions>());
        var inbox = new AnnouncementService(later, Changes());
        Assert.Equal(1, (await inbox.GetInboxAsync(Caller(102))).Unread);
        Assert.Equal(1, (await inbox.GetInboxAsync(Caller(103))).Unread);
        await inbox.MarkReadAsync(Caller(102), sent.Id);
        await inbox.MarkReadAsync(Caller(102), sent.Id);
        var alice = await inbox.GetInboxAsync(Caller(102));
        Assert.Equal(0, alice.Unread);
        Assert.True(Assert.Single(alice.Items).IsRead);
        Assert.Equal(1, (await inbox.GetInboxAsync(Caller(103))).Unread);
        await inbox.MarkReadAsync(Caller(102), int.MaxValue);
        await inbox.MarkReadAsync(Caller(103));
        Assert.Equal(0, (await inbox.GetInboxAsync(Caller(103))).Unread);
    }

    [Fact]
    public async Task Publishing_requires_authenticated_admin_and_current_identity_membership()
    {
        await using var db = await DatabaseAsync();
        var service = new AnnouncementService(db, Changes());
        foreach (var principal in new[] { new ClaimsPrincipal(), Caller(102), Caller(102, "Teacher"), Caller(102, "Admin") })
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.PublishAsync(principal, "Title", "Body"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GetPublishedAsync(principal));
        }
        db.UserRoles.RemoveRange(db.UserRoles);
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.PublishAsync(Caller(101, "Admin"), "Title", "Body"));
        Assert.Empty(db.Announcements);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GetInboxAsync(new ClaimsPrincipal()));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.MarkReadAsync(new ClaimsPrincipal()));
    }

    [Theory]
    [InlineData("", "Body")]
    [InlineData("Title", "   ")]
    public async Task Empty_messages_are_rejected_without_delivering(string title, string body)
    {
        await using var db = await DatabaseAsync();
        var service = new AnnouncementService(db, Changes());
        await Assert.ThrowsAsync<ArgumentException>(() => service.PublishAsync(Caller(101, "Admin"), title, body));
        await Assert.ThrowsAsync<ArgumentException>(() => service.PublishAsync(Caller(101, "Admin"), new string('x', 161), "Body"));
        await Assert.ThrowsAsync<ArgumentException>(() => service.PublishAsync(Caller(101, "Admin"), "Title", new string('x', 4001)));
        Assert.Empty(db.Announcements);
        Assert.Empty(db.UserNotifications);
    }

    [Fact]
    public async Task Pagination_and_mark_all_preserve_other_users_unread_counts()
    {
        await using var db = await DatabaseAsync();
        var service = new AnnouncementService(db, Changes());
        for (var i = 1; i <= 23; i++) await service.PublishAsync(Caller(101, "Admin"), $"Notice {i}", "Message");
        var first = await service.GetInboxAsync(Caller(102));
        var second = await service.GetInboxAsync(Caller(102), 2);
        Assert.Equal(23, first.Unread);
        Assert.Equal(20, first.Items.Count);
        Assert.Equal(3, second.Items.Count);
        Assert.Empty(first.Items.Select(x => x.Id).Intersect(second.Items.Select(x => x.Id)));
        await service.MarkReadAsync(Caller(102));
        Assert.Equal(0, (await service.GetInboxAsync(Caller(102))).Unread);
        Assert.Equal(23, (await service.GetInboxAsync(Caller(103))).Unread);
    }

    [Fact]
    public async Task Disconnected_subscriber_cannot_turn_a_committed_broadcast_into_a_send_error()
    {
        await using var db = await DatabaseAsync();
        var changes = Changes();
        changes.Changed += _ => throw new ObjectDisposedException("circuit");
        int updates = 0;
        changes.Changed += _ => updates++;
        var service = new AnnouncementService(db, changes);
        await service.PublishAsync(Caller(101, "Admin"), "Title", "Body");
        Assert.Equal(1, updates);
        Assert.Equal(3, await db.UserNotifications.CountAsync());
    }
}
