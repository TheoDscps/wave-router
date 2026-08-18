using WaveRouter.Core.Models;

namespace WaveRouter.Core.Abstractions;

/// <summary>Result of loading the store: <see cref="Warning"/> is set when it was corrupted and had to be reset (see docs/use-cases/rule-persistence.md).</summary>
public sealed record RuleStoreLoadResult(RuleStore Store, string? Warning);

public interface IRuleRepository
{
    /// <summary>Returns an empty store on first run (no file yet). Throws <see cref="Errors.RulePersistenceException"/> on unrecoverable errors.</summary>
    Task<RuleStoreLoadResult> LoadAsync();

    /// <summary>Persists the full store, replacing whatever was stored before. Throws <see cref="Errors.RulePersistenceException"/> on failure.</summary>
    Task SaveAsync(RuleStore store);
}
