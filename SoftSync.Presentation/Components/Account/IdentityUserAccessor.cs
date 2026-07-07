using Microsoft.AspNetCore.Identity;
using SoftSync.DAL.Entities;

namespace SoftSync.Presentation.Components.Account;

/// <summary>Resolves the current <see cref="ApplicationUser"/> for a request,
/// redirecting to an error/login page when none is found. Adapted from the
/// Blazor Web App Individual Accounts template.</summary>
public sealed class IdentityUserAccessor(
    UserManager<ApplicationUser> userManager,
    IdentityRedirectManager redirectManager)
{
    public async Task<ApplicationUser> GetRequiredUserAsync(HttpContext context)
    {
        var user = await userManager.GetUserAsync(context.User);
        if (user is null)
        {
            redirectManager.RedirectToWithStatus(
                "Account/Login",
                "Error: Không tìm thấy người dùng. Vui lòng đăng nhập lại.",
                context);
        }
        return user;
    }
}
