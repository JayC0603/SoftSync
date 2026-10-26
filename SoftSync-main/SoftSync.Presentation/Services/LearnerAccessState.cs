using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using SoftSync.BLL.Interfaces;
using SoftSync.Common.Enums;

namespace SoftSync.Presentation.Services;

public enum AssessmentAccessStatus { Unknown, Loading, Incomplete, Complete, Failed }

// Identity comes exclusively from ASP.NET Identity. Assessment status is derived
// from persisted results, never from XP, browser storage or route visits.
public sealed class LearnerAccessState(AuthenticationStateProvider authentication,
    IServiceScopeFactory scopes, ILogger<LearnerAccessState> logger) : IDisposable
{
    public AssessmentAccessStatus Status { get; private set; }
    public int UserId { get; private set; }
    public bool IsAuthenticated { get; private set; }
    public bool IsStaff { get; private set; }
    public bool ProfileComplete { get; private set; }
    public event Action? Changed;
    private Task? pending;
    private int version;
    private bool subscribed;

    public async Task LoadAsync()
    {
        if (!subscribed) { authentication.AuthenticationStateChanged += AuthenticationChanged; subscribed = true; }
        var principal = (await authentication.GetAuthenticationStateAsync()).User;
        var id = int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var parsed) ? parsed : 0;
        if (id != UserId || IsAuthenticated != (principal.Identity?.IsAuthenticated == true)) Invalidate();
        UserId = id;
        IsAuthenticated = principal.Identity?.IsAuthenticated == true && id > 0;
        IsStaff = IsAuthenticated && (principal.IsInRole("Admin") || principal.IsInRole("Teacher"));
        if (!IsAuthenticated || IsStaff) return;
        if (pending is not null) { await pending; return; }
        if (Status is AssessmentAccessStatus.Complete or AssessmentAccessStatus.Incomplete or AssessmentAccessStatus.Failed) return;
        var currentVersion = version;
        pending = LoadCoreAsync(id, currentVersion);
        try { await pending; }
        finally { if (version == currentVersion) pending = null; }
    }

    private async Task LoadCoreAsync(int id, int currentVersion)
    {
        Status = AssessmentAccessStatus.Loading;
        try
        {
            // A private scope prevents parallel layout/page operations sharing DbContext.
            await using var scope = scopes.CreateAsyncScope();
            var assessments = scope.ServiceProvider.GetRequiredService<IAssessmentService>();
            var users = scope.ServiceProvider.GetRequiredService<IUserService>();
            var complete = (await assessments.GetLatestResultsAsync(id, id)).Any();
            var user = await users.GetUserByIdAsync(id);
            if (version != currentVersion) return;
            // Match ProfileSetup: gender is optional and an empty selection has
            // an existing assessment fallback to the active skill banks.
            ProfileComplete = user is not null && !string.IsNullOrWhiteSpace(user.FullName)
                && user.Age > 0 && !string.IsNullOrWhiteSpace(user.Goal);
            Status = complete ? AssessmentAccessStatus.Complete : AssessmentAccessStatus.Incomplete;
        }
        catch (Exception ex)
        {
            if (version != currentVersion) return;
            logger.LogWarning(ex, "Could not verify learner access for user {UserId}", id);
            Status = AssessmentAccessStatus.Failed;
        }
        finally { Changed?.Invoke(); }
    }

    public void Invalidate()
    {
        version++;
        pending = null;
        Status = AssessmentAccessStatus.Unknown;
        ProfileComplete = false;
        IsAuthenticated = false;
        IsStaff = false;
        UserId = 0;
        Changed?.Invoke();
    }

    private void AuthenticationChanged(Task<AuthenticationState> _) => Invalidate();
    public void Dispose() { if (subscribed) authentication.AuthenticationStateChanged -= AuthenticationChanged; }
}

public static class NavigationAccessRules
{
    public const string MoreRoutes = "/profile /settings /mentors /mentor-requests /resources /community /game";
    public static bool IsMoreRoute(string path) => MoreRoutes.Split(' ').Any(root => Under(path, root));
    private static bool Under(string path, string root) => path.Equals(root, StringComparison.OrdinalIgnoreCase)
        || path.StartsWith(root + "/", StringComparison.OrdinalIgnoreCase);

    public static bool RequiresAssessment(string path) => new[] { "/dashboard", "/roadmap", "/roadmap-legacy", "/progress" }
        .Any(root => Under(path, root));

    public static bool BottomNavigationEnabled(string path) => !new[] {
        "/Account", "/assessment", "/assessment-result", "/assessment-complete", "/profile-setup", "/select-skills"
    }.Any(root => Under(path, root));

    public static bool ShowBottomNavigation(string path, bool authenticated, bool staff, AssessmentAccessStatus status)
        => authenticated && BottomNavigationEnabled(path) && (staff || status == AssessmentAccessStatus.Complete);
}
