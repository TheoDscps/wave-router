namespace WaveRouter.Localization;

/// <summary>
/// Minimal, self-contained localization: no external resource files or satellite assemblies, just two
/// in-memory dictionaries (see <see cref="AppStrings"/>) and an event XAML bindings react to, so changing
/// the language takes effect live — no app restart needed.
/// </summary>
public static class LocalizationManager
{
    private static readonly Dictionary<string, Dictionary<string, string>> Catalogs = new()
    {
        ["fr"] = AppStrings.French,
        ["en"] = AppStrings.English,
    };

    /// <summary>Raised whenever <see cref="SetLanguage"/> actually changes the language — <see cref="TranslateExtension"/>
    /// bindings subscribe to this to refresh already-rendered UI.</summary>
    public static event EventHandler? LanguageChanged;

    public static string CurrentLanguage { get; private set; } = "fr";

    public static void SetLanguage(string language)
    {
        if (!Catalogs.ContainsKey(language) || language == CurrentLanguage)
        {
            return;
        }

        CurrentLanguage = language;
        LanguageChanged?.Invoke(null, EventArgs.Empty);
    }

    public static string Translate(string key)
    {
        if (Catalogs.TryGetValue(CurrentLanguage, out var catalog) && catalog.TryGetValue(key, out var value))
        {
            return value;
        }

        // English is the fallback for a missing key — a gap shows up as English text, not a blank or a raw key.
        return Catalogs["en"].TryGetValue(key, out var fallback) ? fallback : key;
    }

    public static string Translate(string key, params object[] args) => string.Format(Translate(key), args);
}
