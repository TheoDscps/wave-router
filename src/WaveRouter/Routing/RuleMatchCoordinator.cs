using WaveRouter.Core.Abstractions;
using WaveRouter.Core.Models;
using WaveRouter.Core.Rules;
using WaveRouter.ViewModels;

namespace WaveRouter.Routing;

/// <summary>
/// Bridges audio session detection to the rule list: for every session the watcher reports, looks up a
/// matching rule and raises <see cref="SessionEvaluated"/> either way. Only saved rules are considered —
/// unsaved drafts in the UI shouldn't trigger anything. Does not yet apply any routing (see
/// docs/use-cases/automatic-routing-enforcement.md — the actual per-app audio switch is a separate step).
/// </summary>
public sealed class RuleMatchCoordinator : IDisposable
{
    private readonly IAudioSessionWatcher _watcher;
    private readonly RuleListViewModel _ruleList;

    public event EventHandler<RuleMatchResult>? SessionEvaluated;

    public RuleMatchCoordinator(IAudioSessionWatcher watcher, RuleListViewModel ruleList)
    {
        _watcher = watcher;
        _ruleList = ruleList;
        _watcher.NewSessionDetected += OnNewSessionDetected;
    }

    public void Start() => _watcher.Start();

    private void OnNewSessionDetected(object? sender, AudioSessionInfo session)
    {
        var savedRules = _ruleList.Rules.Where(r => !r.IsNew).Select(r => r.ToRule()).ToList();
        var match = RuleMatcher.FindMatch(savedRules, session.ProcessName);
        SessionEvaluated?.Invoke(this, new RuleMatchResult(session, match));
    }

    public void Dispose()
    {
        _watcher.NewSessionDetected -= OnNewSessionDetected;
        _watcher.Dispose();
    }
}
