using System.Windows.Forms;
using WaveRouter.Audio;

namespace WaveRouter.Tray;

/// <summary>
/// Owns the system tray icon. The app has no visible window by default (ShutdownMode="OnExplicitShutdown")
/// — double-clicking the icon opens <see cref="MainWindow"/>, "Quitter" ends the process.
/// </summary>
public sealed class TrayIconManager : IDisposable
{
    private readonly NotifyIcon _icon;
    private readonly AudioSessionWatcher _watcher = new();
    private MainWindow? _mainWindow;
    private StyleGuideWindow? _styleGuideWindow;

    public TrayIconManager()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Ouvrir les règles", null, (_, _) => ShowMainWindow());
        menu.Items.Add("Style guide", null, (_, _) => ShowStyleGuide());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Quitter", null, (_, _) => System.Windows.Application.Current.Shutdown());

        _icon = new NotifyIcon
        {
            Icon = System.Drawing.SystemIcons.Application,
            Text = "WaveRouter",
            Visible = true,
            ContextMenuStrip = menu,
        };
        _icon.DoubleClick += (_, _) => ShowMainWindow();
    }

    public void Start()
    {
        _watcher.NewSessionDetected += OnNewSessionDetected;
        _watcher.Start();
    }

    private void ShowMainWindow()
    {
        _mainWindow ??= new MainWindow();
        _mainWindow.Show();
        _mainWindow.Activate();
    }

    private void ShowStyleGuide()
    {
        _styleGuideWindow ??= new StyleGuideWindow();
        _styleGuideWindow.Show();
        _styleGuideWindow.Activate();
    }

    private void OnNewSessionDetected(object? sender, AudioSessionInfo session)
    {
        // Placeholder: routing rule matching lands with the rule engine (see docs/use-cases/automatic-routing-enforcement.md).
        _icon.BalloonTipTitle = "Nouvelle source audio";
        _icon.BalloonTipText = $"{session.DisplayName} ({session.ProcessName})";
        _icon.ShowBalloonTip(3000);
    }

    public void Dispose()
    {
        _watcher.NewSessionDetected -= OnNewSessionDetected;
        _watcher.Dispose();
        _icon.Visible = false;
        _icon.Dispose();
    }
}
