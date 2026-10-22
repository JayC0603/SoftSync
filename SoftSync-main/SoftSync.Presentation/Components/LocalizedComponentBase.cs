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
    protected string Ui(string? text) => L.Ui(text);
    protected string Ui(string? vietnamese, string? english) => L.Ui(L.Current == AppLanguage.Vi ? vietnamese : english);
    protected string UiFormat(string text, params object?[] args) => string.Format(
        System.Globalization.CultureInfo.GetCultureInfo(L.Current == AppLanguage.Vi ? "vi-VN" : "en-US"),
        Ui(text), args);

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
