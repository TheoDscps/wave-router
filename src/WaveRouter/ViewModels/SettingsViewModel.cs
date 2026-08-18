using WaveRouter.Core.Abstractions;
using WaveRouter.Core.Models;
using WaveRouter.Localization;
using WaveRouter.Mvvm;
using WaveRouter.Themes;

namespace WaveRouter.ViewModels;

/// <summary>Backs <see cref="SettingsWindow"/>: theme and language, applied immediately and persisted.</summary>
public sealed class SettingsViewModel : ObservableObject
{
    private readonly IAppSettingsRepository _repository;
    private AppTheme _theme;
    private string _language;

    public SettingsViewModel(IAppSettingsRepository repository, AppSettings initial)
    {
        _repository = repository;
        _theme = initial.Theme == nameof(AppTheme.Light) ? AppTheme.Light : AppTheme.Dark;
        _language = initial.Language;

        SetThemeCommand = new RelayCommand(async theme => await SetThemeAsync((AppTheme)theme!));
        SetLanguageCommand = new RelayCommand(async language => await SetLanguageAsync((string)language!));
    }

    public AppTheme Theme
    {
        get => _theme;
        private set => SetProperty(ref _theme, value);
    }

    public string Language
    {
        get => _language;
        private set => SetProperty(ref _language, value);
    }

    public RelayCommand SetThemeCommand { get; }
    public RelayCommand SetLanguageCommand { get; }

    private async Task SetThemeAsync(AppTheme theme)
    {
        if (theme == Theme)
        {
            return;
        }

        Theme = theme;
        ThemeManager.Apply(theme);
        await PersistAsync();
    }

    private async Task SetLanguageAsync(string language)
    {
        if (language == Language)
        {
            return;
        }

        Language = language;
        LocalizationManager.SetLanguage(language);
        await PersistAsync();
    }

    private Task PersistAsync() => _repository.SaveAsync(new AppSettings(Theme.ToString(), Language));
}
