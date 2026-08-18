namespace WaveRouter.Core.Models;

/// <summary>User-level app preferences — theme and UI language. Kept separate from <see cref="RuleStore"/>
/// since they're a different concern (app preferences vs. routing data) with a different lifecycle.</summary>
public sealed record AppSettings(string Theme, string Language)
{
    public static AppSettings Default { get; } = new("Dark", "fr");
}
