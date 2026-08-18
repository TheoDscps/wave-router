namespace WaveRouter.Core.Models;

/// <summary>Everything persisted: routing rules, plus executables the user chose to never be prompted
/// about again (see docs/use-cases/audio-session-detection.md and automatic-routing-enforcement.md).</summary>
public sealed record RuleStore(IReadOnlyList<Rule> Rules, IReadOnlyList<string> IgnoredExecutables)
{
    public static RuleStore Empty { get; } = new([], []);
}
