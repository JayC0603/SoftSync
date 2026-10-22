using Microsoft.AspNetCore.Components.Authorization;
using SoftSync.BLL.Services;

namespace SoftSync.Presentation.Services;

// A fresh scope per operation keeps header refreshes independent of page DbContexts.
public sealed class NotificationInbox(IServiceScopeFactory scopes, AuthenticationStateProvider auth)
{
    public async Task<NotificationPage> GetAsync(int page = 1, int pageSize = 20, CancellationToken token = default)
    {
        var caller = (await auth.GetAuthenticationStateAsync()).User;
        await using var scope = scopes.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AnnouncementService>().GetInboxAsync(caller, page, pageSize, token);
    }

    public async Task MarkReadAsync(int? id = null)
    {
        var caller = (await auth.GetAuthenticationStateAsync()).User;
        await using var scope = scopes.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<AnnouncementService>().MarkReadAsync(caller, id);
    }

    public async Task<PublishedAnnouncement> PublishAsync(string title, string body)
    {
        var caller = (await auth.GetAuthenticationStateAsync()).User;
        await using var scope = scopes.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AnnouncementService>().PublishAsync(caller, title, body);
    }

    public async Task<IReadOnlyList<PublishedAnnouncement>> GetPublishedAsync()
    {
        var caller = (await auth.GetAuthenticationStateAsync()).User;
        await using var scope = scopes.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AnnouncementService>().GetPublishedAsync(caller);
    }
}
