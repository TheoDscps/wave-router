using WaveRouter.Core.Models;

namespace WaveRouter.Routing;

/// <summary>A detected audio session paired with the rule that matched it, if any.</summary>
public sealed record RuleMatchResult(AudioSessionInfo Session, Rule? MatchedRule);
