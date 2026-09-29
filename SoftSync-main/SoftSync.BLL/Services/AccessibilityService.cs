using SoftSync.BLL.Interfaces;
using SoftSync.Common.Dtos;
using SoftSync.DAL.Repositories;

namespace SoftSync.BLL.Services;

public sealed class AccessibilityService(IUserRepository users) : IAccessibilityService
{
    public async Task<UserDto?> GetUserPreferenceAsync(int authenticatedUserId) =>
        authenticatedUserId <= 0 ? null : await new UserService(users).GetUserByIdAsync(authenticatedUserId);

    public async Task<bool> UpdatePreferenceAsync(int authenticatedUserId, UserDto preference)
    {
        if (authenticatedUserId <= 0 || preference is null) return false;
        var user = await users.GetByIdAsync(authenticatedUserId);
        if (user is null) return false;
        user.PreferredLearningMode = preference.PreferredLearningMode;
        user.LargeText = preference.LargeText;
        user.HighContrast = preference.HighContrast;
        user.ReduceMotion = preference.ReduceMotion;
        user.CaptionEnabled = preference.CaptionEnabled;
        return await users.SaveChangesAsync();
    }
}
