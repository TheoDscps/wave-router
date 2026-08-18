using WaveRouter.Core.Models;

namespace WaveRouter.Core.Abstractions;

/// <summary>Result of loading rules: <see cref="Warning"/> is set when the store was corrupted and had to be reset (see docs/use-cases/rule-persistence.md).</summary>
public sealed record RuleLoadResult(IReadOnlyList<Rule> Rules, string? Warning);

public interface IRuleRepository
{
    /// <summary>Returns an empty list on first run (no file yet). Throws <see cref="Errors.RulePersistenceException"/> on unrecoverable errors.</summary>
    Task<RuleLoadResult> LoadAsync();

    /// <summary>Persists the full rule set, replacing whatever was stored before. Throws <see cref="Errors.RulePersistenceException"/> on failure.</summary>
    Task SaveAsync(IReadOnlyList<Rule> rules);
}
