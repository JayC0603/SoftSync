using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SoftSync.BLL.Interfaces;
using SoftSync.BLL.Services;
using SoftSync.Common.Dtos;
using SoftSync.DAL.Data;
using SoftSync.DAL.Entities;
using SoftSync.DAL.Repositories;
using SoftSync.Presentation.Services;
using Xunit;

namespace SoftSync.Tests;

public class NavigationAccessTests
{
    [Theory]
    [InlineData("/mentors", false, false, AssessmentAccessStatus.Complete, false)]
    [InlineData("/game", true, false, AssessmentAccessStatus.Unknown, false)]
    [InlineData("/game", true, false, AssessmentAccessStatus.Loading, false)]
    [InlineData("/game", true, false, AssessmentAccessStatus.Incomplete, false)]
    [InlineData("/game", true, false, AssessmentAccessStatus.Failed, false)]
    [InlineData("/game", true, false, AssessmentAccessStatus.Complete, true)]
    [InlineData("/mentors/1", true, false, AssessmentAccessStatus.Complete, true)]
    [InlineData("/resources", true, false, AssessmentAccessStatus.Complete, true)]
    [InlineData("/community", true, false, AssessmentAccessStatus.Complete, true)]
    [InlineData("/assessment/100", true, false, AssessmentAccessStatus.Complete, false)]
    [InlineData("/assessment-result/100", true, false, AssessmentAccessStatus.Complete, false)]
    [InlineData("/Account/Login", true, false, AssessmentAccessStatus.Complete, false)]
    [InlineData("/Account/Register", true, false, AssessmentAccessStatus.Complete, false)]
    [InlineData("/profile-setup", true, false, AssessmentAccessStatus.Complete, false)]
    [InlineData("/admin/users", true, true, AssessmentAccessStatus.Unknown, true)]
    [InlineData("/teacher/courses", true, true, AssessmentAccessStatus.Incomplete, true)]
    public void Dock_respects_identity_results_role_and_route(string path, bool auth, bool staff,
        AssessmentAccessStatus status, bool expected) =>
        Assert.Equal(expected, NavigationAccessRules.ShowBottomNavigation(path, auth, staff, status));

    [Theory]
    [InlineData("/roadmap", true)]
    [InlineData("/roadmap/lecture/100/2", true)]
    [InlineData("/roadmap-legacy/100", true)]
    [InlineData("/dashboard", true)]
    [InlineData("/progress/100", true)]
    [InlineData("/profile", false)]
    [InlineData("/settings", false)]
    [InlineData("/mentors", false)]
    [InlineData("/cv", false)]
    [InlineData("/resources", false)]
    [InlineData("/game", false)]
    public void Only_personalized_learning_routes_require_assessment(string path, bool expected) =>
        Assert.Equal(expected, NavigationAccessRules.RequiresAssessment(path));

    [Fact]
    public async Task XP_and_profile_do_not_count_as_completed_assessment()
    {
        await using var services = await Services();
        var access = services.GetRequiredService<LearnerAccessState>();
        await access.LoadAsync();
        Assert.Equal(AssessmentAccessStatus.Incomplete, access.Status);
        Assert.True(access.ProfileComplete); // Unspecified gender is valid in ProfileSetup.
    }

    [Fact]
    public async Task Submission_invalidation_refresh_and_logout_use_saved_results()
    {
        await using var services = await Services();
        var access = services.GetRequiredService<LearnerAccessState>();
        var probe = services.GetRequiredService<Assessments>();
        await access.LoadAsync();
        probe.Complete = true;
        access.Invalidate(); // Assessment only invalidates after successful submission.
        await access.LoadAsync();
        Assert.Equal(AssessmentAccessStatus.Complete, access.Status);
        await using (var refresh = services.CreateAsyncScope())
        {
            await refresh.ServiceProvider.GetRequiredService<LearnerAccessState>().LoadAsync();
            Assert.Equal(AssessmentAccessStatus.Complete, refresh.ServiceProvider.GetRequiredService<LearnerAccessState>().Status);
        }
        services.GetRequiredService<TestAuthentication>().SetUser(0);
        Assert.False(access.IsAuthenticated);
        Assert.Equal(AssessmentAccessStatus.Unknown, access.Status);
        await access.LoadAsync();
        Assert.False(access.IsAuthenticated);
    }

    [Fact]
    public async Task Lookup_error_is_distinct_from_incomplete_and_retry_queries_again()
    {
        await using var services = await Services();
        var probe = services.GetRequiredService<Assessments>();
        probe.Fail = true;
        var access = services.GetRequiredService<LearnerAccessState>();
        await access.LoadAsync();
        Assert.Equal(AssessmentAccessStatus.Failed, access.Status);
        probe.Fail = false;
        probe.Complete = true;
        access.Invalidate();
        await access.LoadAsync();
        Assert.Equal(AssessmentAccessStatus.Complete, access.Status);
    }

    [Fact]
    public async Task Concurrent_consumers_share_lookup_and_late_results_cannot_restore_logged_out_access()
    {
        await using var services = await Services();
        var probe = services.GetRequiredService<Assessments>();
        probe.Pending = new(TaskCreationOptions.RunContinuationsAsynchronously);
        var access = services.GetRequiredService<LearnerAccessState>();
        var first = access.LoadAsync();
        var second = access.LoadAsync();
        Assert.Equal(AssessmentAccessStatus.Loading, access.Status);
        Assert.Equal(1, probe.Reads);
        services.GetRequiredService<TestAuthentication>().SetUser(0);
        probe.Pending.SetResult([new AssessmentResultDto { SkillId = 1 }]);
        await Task.WhenAll(first, second);
        Assert.False(access.IsAuthenticated);
        Assert.Equal(AssessmentAccessStatus.Unknown, access.Status);
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Teacher")]
    public async Task Staff_does_not_require_a_learner_assessment(string role)
    {
        await using var services = await Services();
        services.GetRequiredService<TestAuthentication>().SetUser(100, role);
        var access = services.GetRequiredService<LearnerAccessState>();
        await access.LoadAsync();
        Assert.True(access.IsStaff);
        Assert.Equal(0, services.GetRequiredService<Assessments>().Reads);
    }

    private static async Task<ServiceProvider> Services()
    {
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection().AddLogging()
            .AddDbContext<SoftSyncDbContext>(options => options.UseInMemoryDatabase(dbName))
            .AddSingleton<TestAuthentication>()
            .AddSingleton<AuthenticationStateProvider>(p => p.GetRequiredService<TestAuthentication>())
            .AddSingleton<Assessments>()
            .AddScoped<IAssessmentService>(p => p.GetRequiredService<Assessments>())
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IUserService, UserService>()
            .AddScoped<LearnerAccessState>().BuildServiceProvider();
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SoftSyncDbContext>();
        db.Users.Add(new ApplicationUser { Id = 100, FullName = "Learner", Age = 23, Goal = "communication", ExperiencePoints = 500 });
        await db.SaveChangesAsync();
        return services;
    }

    private sealed class TestAuthentication : AuthenticationStateProvider
    {
        private ClaimsPrincipal user = Principal(100, null);
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(user));
        public void SetUser(int id, string? role = null)
        {
            user = Principal(id, role);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
        private static ClaimsPrincipal Principal(int id, string? role) => id == 0 ? new(new ClaimsIdentity())
            : new(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, id.ToString()), new Claim(ClaimTypes.Role, role ?? "User") }, "Test"));
    }

    private sealed class Assessments : IAssessmentService
    {
        public bool Complete, Fail;
        public int Reads;
        public TaskCompletionSource<IEnumerable<AssessmentResultDto>>? Pending;
        public Task<IEnumerable<AssessmentResultDto>> GetLatestResultsAsync(int authenticatedUserId, int ownerUserId)
        {
            Assert.Equal(authenticatedUserId, ownerUserId);
            Reads++;
            return Pending?.Task ?? (Fail ? Task.FromException<IEnumerable<AssessmentResultDto>>(new InvalidOperationException("Unavailable"))
                : Task.FromResult<IEnumerable<AssessmentResultDto>>(Complete ? [new() { SkillId = 1 }] : []));
        }
        public Task<IEnumerable<AssessmentQuestionDto>> GetAssessmentQuestionsAsync(int userId) => throw new NotSupportedException();
        public Task SubmitAssessmentAsync(int userId, List<UserAnswerDto> answers) => throw new NotSupportedException();
        public Task<IReadOnlyList<AssessmentAttemptDto>> GetHistoryAsync(int authenticatedUserId, int ownerUserId) => throw new NotSupportedException();
    }
}
