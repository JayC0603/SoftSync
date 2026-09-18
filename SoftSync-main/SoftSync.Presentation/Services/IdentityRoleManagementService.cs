using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using SoftSync.BLL.Auth;
using SoftSync.DAL.Entities;

namespace SoftSync.Presentation.Services;

public sealed record ManagedUserRoleDto(int Id, string Email, string DisplayName, bool IsUser, bool IsTeacher, bool IsAdmin);

public sealed class IdentityRoleManagementService(UserManager<ApplicationUser> users, AuthenticationStateProvider authenticationStateProvider)
{
    public async Task<IReadOnlyList<ManagedUserRoleDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var caller = await GetAdminCallerAsync();
        if (caller is null) return [];
        var accounts = await users.Users.AsNoTracking().OrderBy(x => x.Email).ToListAsync(cancellationToken);
        var result = new List<ManagedUserRoleDto>(accounts.Count);
        foreach (var account in accounts)
        {
            var roles = await users.GetRolesAsync(account);
            result.Add(new(account.Id, account.Email ?? account.UserName ?? $"User {account.Id}",
                string.IsNullOrWhiteSpace(account.DisplayName) ? account.FullName : account.DisplayName,
                roles.Contains(CourseAuthorization.UserRole), roles.Contains(CourseAuthorization.TeacherRole), roles.Contains(CourseAuthorization.AdminRole)));
        }
        return result;
    }

    public Task<IdentityResult> AssignTeacherAsync(int targetUserId) =>
        ChangeTeacherRoleAsync(targetUserId, assign: true);

    public Task<IdentityResult> RemoveTeacherAsync(int targetUserId) =>
        ChangeTeacherRoleAsync(targetUserId, assign: false);

    private async Task<IdentityResult> ChangeTeacherRoleAsync(int targetUserId, bool assign)
    {
        var caller = await GetAdminCallerAsync();
        if (caller is null || !CourseAuthorization.CanManageTeacherRoles(true, caller.Value.UserId, targetUserId))
            return IdentityResult.Failed(new IdentityError { Code = "Forbidden", Description = "Only an Admin may manage another user's Teacher role." });

        var target = await users.FindByIdAsync(targetUserId.ToString());
        if (target is null)
            return IdentityResult.Failed(new IdentityError { Code = "UserNotFound", Description = "User not found." });
        if (await users.IsInRoleAsync(target, CourseAuthorization.AdminRole))
            return IdentityResult.Failed(new IdentityError { Code = "AdminProtected", Description = "Admin role membership cannot be changed here." });

        var isTeacher = await users.IsInRoleAsync(target, CourseAuthorization.TeacherRole);
        if (assign)
            return isTeacher ? IdentityResult.Success : await users.AddToRoleAsync(target, CourseAuthorization.TeacherRole);

        if (!isTeacher) return IdentityResult.Success;
        var removed = await users.RemoveFromRoleAsync(target, CourseAuthorization.TeacherRole);
        if (!removed.Succeeded) return removed;
        if (!await users.IsInRoleAsync(target, CourseAuthorization.UserRole))
            return await users.AddToRoleAsync(target, CourseAuthorization.UserRole);
        return IdentityResult.Success;
    }

    private async Task<(int UserId, ApplicationUser Account)?> GetAdminCallerAsync()
    {
        var principal = (await authenticationStateProvider.GetAuthenticationStateAsync()).User;
        var userId = principal.GetUserId();
        if (userId <= 0 || !principal.IsInRole(CourseAuthorization.AdminRole)) return null;
        var caller = await users.FindByIdAsync(userId.ToString());
        return caller is not null && await users.IsInRoleAsync(caller, CourseAuthorization.AdminRole) ? (userId, caller) : null;
    }
}
