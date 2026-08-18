using WaveRouter.Core.Abstractions;
using WaveRouter.Core.Models;
using WaveRouter.Core.Rules;
using WaveRouter.ViewModels;

namespace WaveRouter.Routing;

/// <summary>
/// Bridges audio session detection to the rule list: for every session the watcher reports, looks up a
/// matching rule and, if one is found, applies it via <see cref="IAudioRouter"/>. Only saved rules are
/// considered — unsaved drafts in the UI shouldn't trigger anything. Raises <see cref="SessionEvaluated"/>
/// when a rule matched (or not) so the UI/tray can report what happened, or <see cref="UnknownAppDetected"/>
/// for a session with neither a rule nor an ignore entry — the app should prompt the user for one.
/// See docs/use-cases/automatic-routing-enforcement.md.
/// </summary>
public sealed class RuleMatchCoordinator : IDisposable
{
    private readonly IAudioSessionWatcher _watcher;
    private readonly IAudioRouter _router;
    private readonly RuleListViewModel _ruleList;

    public event EventHandler<RuleMatchResult>? SessionEvaluated;
    public event EventHandler<AudioSessionInfo>? UnknownAppDetected;

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

        if (match is not null)
        {
            var routing = _router.ApplyRule(session.ProcessId, match.TrackName);
            SessionEvaluated?.Invoke(this, new RuleMatchResult(session, match, routing));
            return;
        }

        if (RuleMatcher.IsIgnored(_ruleList.IgnoredExecutables, session.ProcessName))
        {
            return;
        }

        UnknownAppDetected?.Invoke(this, session);
    }

    public void Dispose()
    {
        _watcher.NewSessionDetected -= OnNewSessionDetected;
        _watcher.Dispose();
    }
}
