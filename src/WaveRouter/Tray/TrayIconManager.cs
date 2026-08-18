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
        _matchCoordinator = new RuleMatchCoordinator(new AudioSessionWatcher(), ruleListViewModel);

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

    private void ShowMainWindow()
    {
        _mainWindow ??= new MainWindow(_ruleListViewModel);
        _mainWindow.Show();
        _mainWindow.Activate();
    }

    private void ShowStyleGuide()
    {
        _styleGuideWindow ??= new StyleGuideWindow();
        _styleGuideWindow.Show();
        _styleGuideWindow.Activate();
    }

    private void OnSessionEvaluated(object? sender, RuleMatchResult result)
    {
        // Matching only, for now — actually switching the app's audio output lands with the
        // Windows per-app routing implementation (see docs/use-cases/automatic-routing-enforcement.md).
        if (result.MatchedRule is { } rule)
        {
            _icon.BalloonTipTitle = "Règle trouvée";
            _icon.BalloonTipText = $"{result.Session.DisplayName} → {rule.TrackName} (routage automatique à venir)";
        }
        else
        {
            _icon.BalloonTipTitle = "Nouvelle source audio";
            _icon.BalloonTipText = $"{result.Session.DisplayName} ({result.Session.ProcessName}) — aucune règle";
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
