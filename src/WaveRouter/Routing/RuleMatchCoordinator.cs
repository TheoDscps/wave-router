using WaveRouter.Core.Abstractions;
using WaveRouter.Core.Models;
using WaveRouter.Core.Rules;
using WaveRouter.ViewModels;

namespace WaveRouter.Routing;

/// <summary>
/// Bridges audio session detection to the rule list: for every session the watcher reports, looks up a
/// matching rule and, if one is found, applies it via <see cref="IAudioRouter"/>. Only saved rules are
/// considered — unsaved drafts in the UI shouldn't trigger anything. Raises <see cref="SessionEvaluated"/>
/// either way so the UI/tray can report what happened. See docs/use-cases/automatic-routing-enforcement.md.
/// </summary>
public sealed class RuleMatchCoordinator : IDisposable
{
    private readonly IAudioSessionWatcher _watcher;
    private readonly IAudioRouter _router;
    private readonly RuleListViewModel _ruleList;

    public event EventHandler<RuleMatchResult>? SessionEvaluated;

    public RuleMatchCoordinator(IAudioSessionWatcher watcher, IAudioRouter router, RuleListViewModel ruleList)
    {
        _watcher = watcher;
        _router = router;
        _ruleList = ruleList;
        _watcher.NewSessionDetected += OnNewSessionDetected;
    }

    public void Start() => _watcher.Start();

    private void OnNewSessionDetected(object? sender, AudioSessionInfo session)
    {
        var savedRules = _ruleList.Rules.Where(r => !r.IsNew).Select(r => r.ToRule()).ToList();
        var match = RuleMatcher.FindMatch(savedRules, session.ProcessName);
        var routing = match is not null ? _router.ApplyRule(session.ProcessId, match.TrackName) : null;
        SessionEvaluated?.Invoke(this, new RuleMatchResult(session, match, routing));
    }

    public void Dispose()
    {
        _watcher.NewSessionDetected -= OnNewSessionDetected;
        _watcher.Dispose();
    }
}
