using Microsoft.Extensions.DependencyInjection;
using WaveRouter.Core.Abstractions;
using WaveRouter.Infrastructure.Audio;
using WaveRouter.Infrastructure.Persistence;
using WaveRouter.Infrastructure.Startup;
using WaveRouter.Localization;
using WaveRouter.Themes;
using WaveRouter.Tray;
using WaveRouter.ViewModels;

namespace WaveRouter;

public partial class App : System.Windows.Application
{
    private ServiceProvider? _services;
    private TrayIconManager? _tray;
    private SingleInstanceGuard? _instanceGuard;

    protected override async void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);

        // Fire-and-forget async command handlers (RelayCommand) can't propagate exceptions to their
        // caller — without this, a failure deep in an async chain (e.g. a save) fails completely silently.
        DispatcherUnhandledException += (_, args) =>
        {
            System.Windows.MessageBox.Show(
                args.Exception.ToString(),
                "WaveRouter — unexpected error",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
            args.Handled = true;
        };

        _instanceGuard = new SingleInstanceGuard();
        if (!_instanceGuard.IsFirstInstance)
        {
            _instanceGuard.SignalExistingInstance();
            Shutdown(); // triggers OnExit, which disposes _instanceGuard
            return;
        }

        var services = new ServiceCollection();
        services.AddSingleton<IRuleRepository, JsonRuleRepository>();
        services.AddSingleton<ITrackProvider, WaveLinkTrackProvider>();
        services.AddSingleton<IAppSettingsRepository, JsonAppSettingsRepository>();
        services.AddSingleton<IStartupRegistration, WindowsStartupRegistration>();
        // One shared instance backs both IAudioRouter and IExistingRoutingScanner — no reason to activate
        // the underlying WinRT factory (see PolicyConfigAudioRouter) twice.
        services.AddSingleton<PolicyConfigAudioRouter>();
        services.AddSingleton<IAudioRouter>(sp => sp.GetRequiredService<PolicyConfigAudioRouter>());
        services.AddSingleton<IExistingRoutingScanner>(sp => sp.GetRequiredService<PolicyConfigAudioRouter>());
        services.AddSingleton<IWaveLinkMixerConfigReader, WaveLinkMixerConfigReader>();
        _services = services.BuildServiceProvider();

        var settingsRepository = _services.GetRequiredService<IAppSettingsRepository>();
        var startupRegistration = _services.GetRequiredService<IStartupRegistration>();
        var appSettings = await settingsRepository.LoadAsync();
        ThemeManager.Apply(appSettings.Theme == nameof(AppTheme.Light) ? AppTheme.Light : AppTheme.Dark);
        LocalizationManager.SetLanguage(appSettings.Language);
        var settingsViewModel = new SettingsViewModel(settingsRepository, startupRegistration, appSettings);

        var repository = _services.GetRequiredService<IRuleRepository>();
        var trackProvider = _services.GetRequiredService<ITrackProvider>();
        var routingScanner = _services.GetRequiredService<IExistingRoutingScanner>();
        var mixerConfigReader = _services.GetRequiredService<IWaveLinkMixerConfigReader>();
        var router = _services.GetRequiredService<IAudioRouter>();
        var initialLoad = await repository.LoadAsync();
        var ruleListViewModel = new RuleListViewModel(repository, trackProvider, routingScanner, mixerConfigReader, initialLoad);

        _tray = new TrayIconManager(ruleListViewModel, settingsViewModel, router);
        _tray.Start();
        _instanceGuard.ListenForActivationRequests(() => _tray.ShowMainWindow());
    }

    protected override void OnExit(System.Windows.ExitEventArgs e)
    {
        _tray?.Dispose();
        _services?.Dispose();
        _instanceGuard?.Dispose();
        base.OnExit(e);
    }
}
