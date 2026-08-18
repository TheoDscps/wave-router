using System.Windows.Forms;
using WaveRouter.Infrastructure.Audio;
using WaveRouter.Routing;
using WaveRouter.ViewModels;

namespace WaveRouter.Tray;

/// <summary>
/// Owns the system tray icon. The app has no visible window by default (ShutdownMode="OnExplicitShutdown")
/// — double-clicking the icon opens <see cref="MainWindow"/>, "Quitter" ends the process.
/// </summary>
public sealed class TrayIconManager : IDisposable
{
    private readonly RuleListViewModel _ruleListViewModel;
    private readonly NotifyIcon _icon;
    private readonly RuleMatchCoordinator _matchCoordinator;
    private MainWindow? _mainWindow;
    private StyleGuideWindow? _styleGuideWindow;

    public TrayIconManager(RuleListViewModel ruleListViewModel)
    {
        _ruleListViewModel = ruleListViewModel;
        _matchCoordinator = new RuleMatchCoordinator(new AudioSessionWatcher(), new PolicyConfigAudioRouter(), ruleListViewModel);

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
        _matchCoordinator.SessionEvaluated += OnSessionEvaluated;
        _matchCoordinator.Start();
    }

    /// <summary>Also called when another launched instance asks to be shown instead — see <see cref="SingleInstanceGuard"/>.</summary>
    public void ShowMainWindow()
    {
        _mainWindow ??= new MainWindow(_ruleListViewModel);
        _mainWindow.Show();
        _mainWindow.WindowState = System.Windows.WindowState.Normal;

        // Activate() alone doesn't reliably steal focus when the request came from another process
        // (e.g. a second launch) — the classic fix is to force-then-release topmost.
        _mainWindow.Topmost = true;
        _mainWindow.Activate();
        _mainWindow.Topmost = false;
    }

    private void ShowStyleGuide()
    {
        _styleGuideWindow ??= new StyleGuideWindow();
        _styleGuideWindow.Show();
        _styleGuideWindow.Activate();
    }

    private void OnSessionEvaluated(object? sender, RuleMatchResult result)
    {
        if (result.MatchedRule is not { } rule)
        {
            _icon.BalloonTipTitle = "Nouvelle source audio";
            _icon.BalloonTipText = $"{result.Session.DisplayName} ({result.Session.ProcessName}) — aucune règle";
            _icon.BalloonTipIcon = ToolTipIcon.None;
        }
        else if (result.Routing is { Success: true })
        {
            _icon.BalloonTipTitle = "Routage effectué";
            _icon.BalloonTipText = $"{result.Session.DisplayName} → {rule.TrackName}";
            _icon.BalloonTipIcon = ToolTipIcon.Info;
        }
        else
        {
            _icon.BalloonTipTitle = "Échec du routage";
            _icon.BalloonTipText = $"{result.Session.DisplayName} → {rule.TrackName} : {result.Routing?.ErrorMessage}";
            _icon.BalloonTipIcon = ToolTipIcon.Warning;
        }

        _icon.ShowBalloonTip(3000);
    }

    public void Dispose()
    {
        _matchCoordinator.SessionEvaluated -= OnSessionEvaluated;
        _matchCoordinator.Dispose();
        _icon.Visible = false;
        _icon.Dispose();
    }
}
