namespace WaveRouter.Core.Models;

/// <summary>User-level app preferences — theme, UI language, and whether routing balloon tips are shown.
/// Kept separate from <see cref="RuleStore"/> since they're a different concern (app preferences vs.
/// routing data) with a different lifecycle. <paramref name="ShowRoutingNotifications"/> defaults to true
/// so a settings.json written before this field existed still deserializes to the same behavior it had.</summary>
public sealed record AppSettings(string Theme, string Language, bool ShowRoutingNotifications = true)
{
    public static AppSettings Default { get; } = new("Dark", "fr");
}
