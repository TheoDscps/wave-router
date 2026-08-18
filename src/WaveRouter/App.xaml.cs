using Microsoft.Extensions.DependencyInjection;
using WaveRouter.Core.Abstractions;
using WaveRouter.Infrastructure.Audio;
using WaveRouter.Infrastructure.Persistence;
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

        _instanceGuard = new SingleInstanceGuard();
        if (!_instanceGuard.IsFirstInstance)
        {
            _instanceGuard.SignalExistingInstance();
            Shutdown(); // triggers OnExit, which disposes _instanceGuard
            return;
        }

        ThemeManager.Apply(AppTheme.Dark);

        var services = new ServiceCollection();
        services.AddSingleton<IRuleRepository, JsonRuleRepository>();
        services.AddSingleton<ITrackProvider, WaveLinkTrackProvider>();
        _services = services.BuildServiceProvider();

        var repository = _services.GetRequiredService<IRuleRepository>();
        var trackProvider = _services.GetRequiredService<ITrackProvider>();
        var initialLoad = await repository.LoadAsync();
        var ruleListViewModel = new RuleListViewModel(repository, trackProvider, initialLoad);

        _tray = new TrayIconManager(ruleListViewModel);
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
