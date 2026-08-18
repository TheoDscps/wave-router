using Microsoft.Extensions.DependencyInjection;
using WaveRouter.Core.Abstractions;
using WaveRouter.Infrastructure.Persistence;
using WaveRouter.Themes;
using WaveRouter.Tray;
using WaveRouter.ViewModels;

namespace WaveRouter;

public partial class App : System.Windows.Application
{
    private ServiceProvider? _services;
    private TrayIconManager? _tray;

    protected override async void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);

        ThemeManager.Apply(AppTheme.Dark);

        var services = new ServiceCollection();
        services.AddSingleton<IRuleRepository, JsonRuleRepository>();
        _services = services.BuildServiceProvider();

        var repository = _services.GetRequiredService<IRuleRepository>();
        var initialLoad = await repository.LoadAsync();
        var ruleListViewModel = new RuleListViewModel(repository, initialLoad);

        _tray = new TrayIconManager(ruleListViewModel);
        _tray.Start();
    }

    protected override void OnExit(System.Windows.ExitEventArgs e)
    {
        _tray?.Dispose();
        _services?.Dispose();
        base.OnExit(e);
    }
}
