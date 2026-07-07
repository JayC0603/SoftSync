using Microsoft.AspNetCore.Components;
using SoftSync.Presentation.Services;

namespace SoftSync.Presentation.Components;

/// <summary>
/// Base class for components/pages that display translated text. Subscribes to
/// <see cref="LocalizationService.OnChanged"/> so the component re-renders when
/// the language switches — no page reload needed. Use <c>@L["key"]</c> in markup.
/// </summary>
public abstract class LocalizedComponentBase : ComponentBase, IDisposable
{
    [Inject] protected LocalizationService L { get; set; } = default!;

    protected override void OnInitialized()
    {
        L.OnChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged() => InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        L.OnChanged -= OnLanguageChanged;
        GC.SuppressFinalize(this);
    }
}
