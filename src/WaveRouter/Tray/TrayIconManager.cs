using System.Windows.Forms;
using WaveRouter.Core.Abstractions;
using WaveRouter.Core.History;
using WaveRouter.Core.Models;
using WaveRouter.Infrastructure.Audio;
using WaveRouter.Localization;
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
    private readonly SettingsViewModel _settingsViewModel;
    private readonly HistoryViewModel _historyViewModel;
    private readonly RoutingHistory _routingHistory;
    private readonly NotifyIcon _icon;
    private readonly ToolStripMenuItem _openRulesItem;
    private readonly ToolStripMenuItem _historyItem;
    private readonly ToolStripMenuItem _settingsItem;
    private readonly ToolStripMenuItem _quitItem;
    private readonly RuleMatchCoordinator _matchCoordinator;
    private readonly NewAppPromptCoordinator _promptCoordinator;
    private readonly WaveLinkSyncCoordinator _syncCoordinator;
    private MainWindow? _mainWindow;
    private StyleGuideWindow? _styleGuideWindow;
    private SettingsWindow? _settingsWindow;
    private HistoryWindow? _historyWindow;

    public TrayIconManager(RuleListViewModel ruleListViewModel, SettingsViewModel settingsViewModel, HistoryViewModel historyViewModel, RoutingHistory routingHistory, IAudioRouter router)
    {
        _ruleListViewModel = ruleListViewModel;
        _settingsViewModel = settingsViewModel;
        _historyViewModel = historyViewModel;
        _routingHistory = routingHistory;
        _matchCoordinator = new RuleMatchCoordinator(new AudioSessionWatcher(), router, ruleListViewModel);
        _promptCoordinator = new NewAppPromptCoordinator(_matchCoordinator, router, ruleListViewModel);
        _syncCoordinator = new WaveLinkSyncCoordinator(ruleListViewModel);

        var menu = new ContextMenuStrip();
        _openRulesItem = new ToolStripMenuItem(LocalizationManager.Translate("Tray.OpenRules"), null, (_, _) => ShowMainWindow());
        _historyItem = new ToolStripMenuItem(LocalizationManager.Translate("Tray.History"), null, (_, _) => ShowHistoryWindow());
        _settingsItem = new ToolStripMenuItem(LocalizationManager.Translate("Tray.Settings"), null, (_, _) => ShowSettingsWindow());
        _quitItem = new ToolStripMenuItem(LocalizationManager.Translate("Tray.Quit"), null, (_, _) => System.Windows.Application.Current.Shutdown());
        menu.Items.Add(_openRulesItem);
        menu.Items.Add(_historyItem);
        menu.Items.Add(_settingsItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(_quitItem);

        _icon = new NotifyIcon
        {
            // Extracted from the exe's own embedded icon (see <ApplicationIcon> in WaveRouter.csproj) rather
            // than duplicating the .ico as a loose content file.
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath)
                ?? System.Drawing.SystemIcons.Application,
            Text = "WaveRouter",
            Visible = true,
            ContextMenuStrip = menu,
        };
        _icon.DoubleClick += (_, _) => ShowMainWindow();
        LocalizationManager.LanguageChanged += OnLanguageChanged;
    }

    public void Start()
    {
        _matchCoordinator.SessionEvaluated += OnSessionEvaluated;
        _promptCoordinator.RoutingApplied += OnSessionEvaluated;
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

    private void ShowSettingsWindow()
    {
        _settingsWindow ??= new SettingsWindow(_settingsViewModel);
        _settingsWindow.Show();
        _settingsWindow.Activate();
    }

    private void ShowHistoryWindow()
    {
        _historyWindow ??= new HistoryWindow(_historyViewModel);
        _historyWindow.Show();
        _historyWindow.Activate();
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        _openRulesItem.Text = LocalizationManager.Translate("Tray.OpenRules");
        _historyItem.Text = LocalizationManager.Translate("Tray.History");
        _settingsItem.Text = LocalizationManager.Translate("Tray.Settings");
        _quitItem.Text = LocalizationManager.Translate("Tray.Quit");
    }

    private void OnSessionEvaluated(object? sender, RuleMatchResult result)
    {
        if (result.MatchedRule is not { } rule)
        {
            return;
        }

        var success = result.Routing is { Success: true };
        _routingHistory.Record(new RoutingHistoryEntry(
            DateTime.Now, result.Session.DisplayName, result.Session.ProcessName, rule.TrackName, success, result.Routing?.ErrorMessage));

        if (!_settingsViewModel.ShowNotifications)
        {
            return;
        }

        if (success)
        {
            _icon.BalloonTipTitle = LocalizationManager.Translate("Tray.RoutingDoneTitle");
            _icon.BalloonTipText = $"{result.Session.DisplayName} → {rule.TrackName}";
            _icon.BalloonTipIcon = ToolTipIcon.Info;
        }
        else
        {
            _icon.BalloonTipTitle = LocalizationManager.Translate("Tray.RoutingFailedTitle");
            _icon.BalloonTipText = $"{result.Session.DisplayName} → {rule.TrackName} : {result.Routing?.ErrorMessage}";
            _icon.BalloonTipIcon = ToolTipIcon.Warning;
        }

        _icon.ShowBalloonTip(3000);
    }

    public void Dispose()
    {
        LocalizationManager.LanguageChanged -= OnLanguageChanged;
        _matchCoordinator.SessionEvaluated -= OnSessionEvaluated;
        _promptCoordinator.RoutingApplied -= OnSessionEvaluated;
        _syncCoordinator.Dispose();
        _promptCoordinator.Dispose();
        _matchCoordinator.Dispose();
        _icon.Visible = false;
        _icon.Dispose();
    }
}
