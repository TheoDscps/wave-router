using WaveRouter.Themes;
using WaveRouter.Tray;

namespace WaveRouter;

public partial class App : System.Windows.Application
{
    private TrayIconManager? _tray;

    protected override void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);

        ThemeManager.Apply(AppTheme.Dark);

        _tray = new TrayIconManager();
        _tray.Start();
    }

    protected override void OnExit(System.Windows.ExitEventArgs e)
    {
        _tray?.Dispose();
        base.OnExit(e);
    }
}
