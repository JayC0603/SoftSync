using Microsoft.AspNetCore.Components.Authorization;
using SoftSync.BLL.Interfaces;

namespace SoftSync.Presentation.Services;

public sealed class CurrentScheduleUser(AuthenticationStateProvider authentication) : ICurrentScheduleUser
{
    public Task<int> GetUserIdAsync() => authentication.GetUserIdAsync();
}
