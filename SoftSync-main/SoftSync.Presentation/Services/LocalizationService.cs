namespace SoftSync.Presentation.Services;

/// <summary>Supported UI languages.</summary>
public enum AppLanguage
{
    En,
    Vi
}

/// <summary>
/// Holds the current UI language and resolves translation keys. Scoped per
/// Blazor circuit (per user). Components subscribe to <see cref="OnChanged"/>
/// so a language switch re-renders the UI without a full page reload.
/// </summary>
public class LocalizationService
{
    public AppLanguage Current { get; private set; } = AppLanguage.En;

    /// <summary>Raised whenever the language changes.</summary>
    public event Action? OnChanged;

    /// <summary>Look up a translation key for the current language.</summary>
    /// <remarks>Falls back to the English value, then to the raw key if missing.</remarks>
    public string this[string key] => Translations.Get(key, Current);

    /// <summary>Convenience method identical to the indexer.</summary>
    public string T(string key) => Translations.Get(key, Current);

    public void SetLanguage(AppLanguage language)
    {
        if (Current == language) return;
        Current = language;
        OnChanged?.Invoke();
    }

    public void Toggle() => SetLanguage(Current == AppLanguage.En ? AppLanguage.Vi : AppLanguage.En);

    /// <summary>Parse a stored culture string (e.g. from localStorage) into a language.</summary>
    public static AppLanguage Parse(string? value) =>
        string.Equals(value, "vi", StringComparison.OrdinalIgnoreCase) ? AppLanguage.Vi : AppLanguage.En;

    /// <summary>The short code persisted to localStorage.</summary>
    public string Code => Current == AppLanguage.Vi ? "vi" : "en";
}
