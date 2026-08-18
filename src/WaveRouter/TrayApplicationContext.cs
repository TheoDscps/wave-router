using WaveRouter.Audio;

namespace WaveRouter;

/// <summary>
/// No visible main window — the app lives entirely in the system tray.
/// </summary>
public sealed class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _trayIcon;
    private readonly AudioSessionWatcher _watcher = new();

    public TrayApplicationContext()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Quitter", null, (_, _) => ExitApp());

        _trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "WaveRouter",
            Visible = true,
            ContextMenuStrip = menu,
        };

        _watcher.NewSessionDetected += OnNewSessionDetected;
        _watcher.Start();
    }

    private void OnNewSessionDetected(object? sender, AudioSessionInfo session)
    {
        // Placeholder: for now just notify. Next step is a popup to pick a routing target.
        _trayIcon.BalloonTipTitle = "Nouvelle source audio";
        _trayIcon.BalloonTipText = $"{session.DisplayName} ({session.ProcessName})";
        _trayIcon.ShowBalloonTip(3000);
    }

    private void ExitApp()
    {
        _watcher.NewSessionDetected -= OnNewSessionDetected;
        _watcher.Dispose();
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        Application.Exit();
    }
}
