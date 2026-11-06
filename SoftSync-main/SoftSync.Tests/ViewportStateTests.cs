using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using SoftSync.Presentation.Components.Shared;
using SoftSync.Presentation.Services;
using Xunit;

namespace SoftSync.Tests;

public sealed class ViewportStateTests
{
    [Theory]
    [InlineData(true, "desktop")]
    [InlineData(false, "mobile")]
    public async Task Routes_render_the_known_view_without_mounting_the_other_branch(bool desktop, string expected)
    {
        var viewport = new ViewportState(desktop);
        await using var services = new ServiceCollection().AddLogging().AddSingleton<LocalizationService>()
            .AddSingleton(viewport).AddSingleton<IJSRuntime, NoBrowserJs>().BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(DesktopVariant.Desktop)] = (RenderFragment)(builder => builder.AddContent(0, "desktop")),
                [nameof(DesktopVariant.Mobile)] = (RenderFragment)(builder => builder.AddContent(0, "mobile"))
            });
            var firstRoute = await renderer.RenderComponentAsync<DesktopVariant>(parameters);
            Assert.Equal(expected, firstRoute.ToHtmlString());
            var nextRoute = await renderer.RenderComponentAsync<DesktopVariant>(parameters);
            Assert.Equal(expected, nextRoute.ToHtmlString());
            viewport.IsDesktop = !desktop;
            var resizedRoute = await renderer.RenderComponentAsync<DesktopVariant>(parameters);
            Assert.Equal(desktop ? "mobile" : "desktop", resizedRoute.ToHtmlString());
        });
    }

    private sealed class NoBrowserJs : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => throw new InvalidOperationException("First render must not wait for JavaScript.");
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => InvokeAsync<TValue>(identifier, args);
    }
}
