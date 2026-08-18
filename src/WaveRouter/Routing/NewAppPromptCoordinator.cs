using WaveRouter.Core.Abstractions;
using WaveRouter.Core.Models;
using WaveRouter.ViewModels;

namespace WaveRouter.Routing;

/// <summary>
/// Turns <see cref="RuleMatchCoordinator.UnknownAppDetected"/> into a "route this app?" popup, one at a
/// time (queued — several apps starting near-boot shouldn't stack several windows). Once the user picks a
/// track, applies routing to the session immediately (it's already playing) and raises
/// <see cref="RoutingApplied"/> so the tray can notify, same as for a pre-existing rule.
/// </summary>
public sealed class NewAppPromptCoordinator : IDisposable
{
    private readonly RuleMatchCoordinator _matchCoordinator;
    private readonly IAudioRouter _router;
    private readonly RuleListViewModel _ruleList;
    private readonly Queue<AudioSessionInfo> _queue = new();
    private NewAppPromptWindow? _currentWindow;

    public event EventHandler<RuleMatchResult>? RoutingApplied;

    public NewAppPromptCoordinator(RuleMatchCoordinator matchCoordinator, IAudioRouter router, RuleListViewModel ruleList)
    {
        _matchCoordinator = matchCoordinator;
        _router = router;
        _ruleList = ruleList;
        _matchCoordinator.UnknownAppDetected += OnUnknownAppDetected;
    }

    private void OnUnknownAppDetected(object? sender, AudioSessionInfo session)
    {
        // The watcher raises this off the UI thread — window creation must happen on the dispatcher.
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            _queue.Enqueue(session);
            ShowNextIfIdle();
        });
    }

    private void ShowNextIfIdle()
    {
        if (_currentWindow is not null || _queue.Count == 0)
        {
            return;
        }

        var session = _queue.Dequeue();
        var viewModel = new NewAppPromptViewModel(session, _ruleList);
        viewModel.TrackChosen += (_, track) =>
        {
            var result = _router.ApplyRule(session.ProcessId, track);
            RoutingApplied?.Invoke(this, new RuleMatchResult(session, new Rule(session.ProcessName, track), result));
        };

        _currentWindow = new NewAppPromptWindow(viewModel);
        _currentWindow.Closed += (_, _) =>
        {
            _currentWindow = null;
            ShowNextIfIdle();
        };
        _currentWindow.Show();
    }

    public void Dispose()
    {
        _matchCoordinator.UnknownAppDetected -= OnUnknownAppDetected;
        _currentWindow?.Close();
    }
}
