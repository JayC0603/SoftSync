using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SoftSync.DAL.Data;
using SoftSync.DAL.Entities;
using SoftSync.Presentation.Services;
using Xunit;

namespace SoftSync.Tests;

public sealed class EmailRegistrationTests
{
    [Fact]
    public async Task Duplicate_email_ignores_case_and_spaces_and_preserves_existing_account()
    {
        await using var provider = Services();
        await using var scope = provider.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var service = scope.ServiceProvider.GetRequiredService<EmailRegistrationService>();
        var original = new ApplicationUser { Email = "owner@example.com", FullName = "Original owner" };
        Assert.True((await service.CreateAsync(original, "Original@123")).Succeeded);
        var hash = original.PasswordHash;
        var stamp = original.SecurityStamp;
        var attempted = new ApplicationUser { Email = "  OWNER@EXAMPLE.COM  ", FullName = "Replacement name" };

        var result = await service.CreateAsync(attempted, "Different@456");

        Assert.Equal(EmailRegistrationService.DuplicateAccount, Assert.Single(result.Errors).Code);
        var db = scope.ServiceProvider.GetRequiredService<SoftSyncDbContext>();
        db.ChangeTracker.Clear();
        var saved = Assert.Single(await db.Users.ToListAsync());
        Assert.Equal(original.Id, saved.Id);
        Assert.Equal("Original owner", saved.FullName);
        Assert.Equal(hash, saved.PasswordHash);
        Assert.Equal(stamp, saved.SecurityStamp);
        Assert.True(await users.CheckPasswordAsync(saved, "Original@123"));
        Assert.False(await users.CheckPasswordAsync(saved, "Different@456"));
    }

    [Fact]
    public async Task New_email_creates_one_account_and_invalid_password_creates_none()
    {
        await using var provider = Services();
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<EmailRegistrationService>();
        var user = new ApplicationUser { Email = "  new@example.com " };
        Assert.True((await service.CreateAsync(user, "NewUser@123")).Succeeded);
        Assert.Equal("new@example.com", user.Email);
        Assert.Equal("new@example.com", user.UserName);
        Assert.Equal("NEW@EXAMPLE.COM", user.NormalizedEmail);
        var invalid = await service.CreateAsync(new() { Email = "invalid@example.com" }, "x");
        Assert.False(invalid.Succeeded);
        Assert.DoesNotContain(invalid.Errors, e => e.Code == EmailRegistrationService.DuplicateAccount);
        Assert.Single(await scope.ServiceProvider.GetRequiredService<SoftSyncDbContext>().Users.ToListAsync());
    }

    [Theory]
    [InlineData("UserNameIndex")]
    [InlineData("EmailIndex")]
    public async Task Unique_constraint_race_returns_recoverable_error_and_detaches_failed_insert(string constraint)
    {
        await using var provider = Services(new InsertFailure(constraint));
        await using var scope = provider.CreateAsyncScope();
        var user = new ApplicationUser { Email = "race@example.com" };
        var result = await scope.ServiceProvider.GetRequiredService<EmailRegistrationService>().CreateAsync(user, "User@12345");
        Assert.Equal(EmailRegistrationService.DuplicateAccount, Assert.Single(result.Errors).Code);
        var db = scope.ServiceProvider.GetRequiredService<SoftSyncDbContext>();
        Assert.Equal(EntityState.Detached, db.Entry(user).State);
        Assert.Empty(await db.Users.ToListAsync());
    }

    [Fact]
    public async Task Unrelated_database_failure_is_not_reported_as_duplicate_account()
    {
        await using var provider = Services(new InsertFailure("SomeOtherConstraint"));
        await using var scope = provider.CreateAsyncScope();
        await Assert.ThrowsAsync<DbUpdateException>(() => scope.ServiceProvider.GetRequiredService<EmailRegistrationService>()
            .CreateAsync(new() { Email = "new@example.com" }, "User@12345"));
    }

    private static ServiceProvider Services(SaveChangesInterceptor? interceptor = null)
    {
        var name = Guid.NewGuid().ToString();
        var services = new ServiceCollection().AddLogging();
        services.AddDbContext<SoftSyncDbContext>(o =>
        {
            o.UseInMemoryDatabase(name);
            if (interceptor is not null) o.AddInterceptors(interceptor);
        });
        services.AddIdentityCore<ApplicationUser>(o => o.User.RequireUniqueEmail = true)
            .AddRoles<IdentityRole<int>>().AddEntityFrameworkStores<SoftSyncDbContext>();
        services.AddScoped<EmailRegistrationService>();
        return services.BuildServiceProvider();
    }

    private sealed class InsertFailure(string constraint) : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
            InterceptionResult<int> result, CancellationToken cancellationToken = default) =>
            throw new DbUpdateException("Simulated concurrent insert", new PostgresException(
                "duplicate key", "ERROR", "ERROR", PostgresErrorCodes.UniqueViolation,
                tableName: "AspNetUsers", constraintName: constraint));
    }
}
