using WaveRouter.Core.Abstractions;
using WaveRouter.Core.Models;
using WaveRouter.Localization;
using WaveRouter.Mvvm;
using WaveRouter.Themes;
using MessageBox = System.Windows.MessageBox;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace WaveRouter.ViewModels;

/// <summary>Backs <see cref="SettingsWindow"/>: theme, language, notification toggle, and Windows startup
/// registration — all applied immediately. Theme/language/notifications are persisted to settings.json;
/// startup registration has no separate persisted flag, the Windows Run registry key itself is the source
/// of truth (see <see cref="IStartupRegistration"/>).</summary>
public sealed class SettingsViewModel : ObservableObject
{
    private readonly IAppSettingsRepository _repository;
    private readonly IStartupRegistration _startupRegistration;
    private AppTheme _theme;
    private string _language;
    private bool _startWithWindows;
    private bool _showNotifications;

    public SettingsViewModel(IAppSettingsRepository repository, IStartupRegistration startupRegistration, AppSettings initial)
    {
        _repository = repository;
        _startupRegistration = startupRegistration;
        _theme = initial.Theme == nameof(AppTheme.Light) ? AppTheme.Light : AppTheme.Dark;
        _language = initial.Language;
        _startWithWindows = startupRegistration.IsEnabled();
        _showNotifications = initial.ShowRoutingNotifications;

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

    /// <summary>Two-way bound to the Settings window checkbox. Applies immediately on set; if the
    /// registry write fails (e.g. permission issue), reverts and notifies rather than lying about the
    /// actual state (see docs/use-cases/windows-autostart.md).</summary>
    public bool StartWithWindows
    {
        get => _startWithWindows;
        set
        {
            if (value == _startWithWindows)
            {
                return;
            }

            if (_startupRegistration.SetEnabled(value))
            {
                _startWithWindows = value;
                RaisePropertyChanged();
            }
            else
            {
                RaisePropertyChanged();
                MessageBox.Show(
                    LocalizationManager.Translate("Settings.StartWithWindowsFailedMessage"),
                    LocalizationManager.Translate("Settings.StartWithWindowsFailedTitle"),
                    System.Windows.MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
    }

    /// <summary>Two-way bound to the Settings window checkbox. Only gates the success/failure balloon
    /// tips (see <see cref="Tray.TrayIconManager"/>) — routing itself and the history log are unaffected.</summary>
    public bool ShowNotifications
    {
        get => _showNotifications;
        set
        {
            if (SetProperty(ref _showNotifications, value))
            {
                _ = PersistAsync();
            }
        }
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

    private Task PersistAsync() => _repository.SaveAsync(new AppSettings(Theme.ToString(), Language, ShowNotifications));
}
