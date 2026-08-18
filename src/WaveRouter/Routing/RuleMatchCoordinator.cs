using WaveRouter.Core.Abstractions;
using WaveRouter.Core.Models;
using WaveRouter.Core.Rules;
using WaveRouter.ViewModels;

namespace WaveRouter.Routing;

/// <summary>
/// Bridges audio session detection to the rule list. Genuinely new activity (<see cref="IAudioSessionWatcher.NewSessionDetected"/>)
/// gets the full treatment: apply a matching rule, or prompt via <see cref="UnknownAppDetected"/> if
/// there's neither a rule nor an ignore entry. Sessions that were already playing before WaveRouter
/// started (<see cref="IAudioSessionWatcher.ExistingActiveSessionDetected"/>) only get matched against
/// existing rules — never prompted, since popping up a dialog for everything already open when the app
/// launches would be spam, not the "just launched an app" moment the prompt is for. Only saved rules are
/// considered — unsaved drafts in the UI shouldn't trigger anything. See docs/use-cases/automatic-routing-enforcement.md.
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
        _watcher.ExistingActiveSessionDetected += OnExistingActiveSessionDetected;
    }

    public void Start() => _watcher.Start();

    private void OnNewSessionDetected(object? sender, AudioSessionInfo session)
    {
        var match = TryApplyMatch(session);
        if (match is not null)
        {
            return;
        }

        if (RuleMatcher.IsSystemProcess(session.ProcessName) || RuleMatcher.IsIgnored(_ruleList.IgnoredExecutables, session.ProcessName))
        {
            return;
        }

        UnknownAppDetected?.Invoke(this, session);
    }

    private void OnExistingActiveSessionDetected(object? sender, AudioSessionInfo session) => TryApplyMatch(session);

    /// <summary>Applies a matching rule if one exists and raises <see cref="SessionEvaluated"/>. Returns
    /// the matched rule, or null if nothing matched.</summary>
    private Rule? TryApplyMatch(AudioSessionInfo session)
    {
        var savedRules = _ruleList.Rules.Where(r => !r.IsNew).Select(r => r.ToRule()).ToList();
        var match = RuleMatcher.FindMatch(savedRules, session.ProcessName);
        if (match is null)
        {
            return null;
        }

        var routing = _router.ApplyRule(session.ProcessId, match.TrackName);
        SessionEvaluated?.Invoke(this, new RuleMatchResult(session, match, routing));
        return match;
    }

    public void Dispose()
    {
        _watcher.NewSessionDetected -= OnNewSessionDetected;
        _watcher.ExistingActiveSessionDetected -= OnExistingActiveSessionDetected;
        _watcher.Dispose();
    }
}
