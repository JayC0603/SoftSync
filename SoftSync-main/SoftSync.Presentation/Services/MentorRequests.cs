using Microsoft.AspNetCore.Components.Authorization;
using SoftSync.BLL.Services;

namespace SoftSync.Presentation.Services;

public sealed class MentorRequests(IServiceScopeFactory scopes, AuthenticationStateProvider auth)
{
    public async Task<IReadOnlyList<MentorRequestView>> ListAsync(bool manage)
    {
        var caller = (await auth.GetAuthenticationStateAsync()).User;
        await using var scope = scopes.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<MentorRequestWorkflow>().ListAsync(caller, manage);
    }

    public async Task<bool> RespondAsync(int id, string status, string reply)
    {
        var caller = (await auth.GetAuthenticationStateAsync()).User;
        await using var scope = scopes.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<MentorRequestWorkflow>().RespondAsync(caller, id, status, reply);
    }
}
