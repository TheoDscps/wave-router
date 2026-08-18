using WaveRouter.Core.Models;

namespace WaveRouter.Routing;

/// <summary>A detected audio session, the rule that matched it (if any), and the outcome of applying
/// it (null when no rule matched — no routing was attempted).</summary>
public sealed record RuleMatchResult(AudioSessionInfo Session, Rule? MatchedRule, RoutingResult? Routing);
