using WaveRouter.Core.Models;

namespace WaveRouter.Core.Rules;

/// <summary>Finds the rule matching a detected process. See docs/use-cases/automatic-routing-enforcement.md.</summary>
public static class RuleMatcher
{
    public static Rule? FindMatch(IReadOnlyList<Rule> rules, string processName)
    {
        var normalizedProcessName = Normalize(processName);
        foreach (var rule in rules)
        {
            if (string.Equals(Normalize(rule.ExecutableName), normalizedProcessName, StringComparison.OrdinalIgnoreCase))
            {
                return rule;
            }
        }

        return null;
    }

    /// <summary>Rules are authored with a ".exe" suffix (e.g. "discord.exe") but <see cref="System.Diagnostics.Process.ProcessName"/>
    /// never includes it — strip it from both sides so "discord.exe" matches "discord".</summary>
    private static string Normalize(string executableOrProcessName)
    {
        var trimmed = executableOrProcessName.Trim();
        return trimmed.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
            ? trimmed[..^4]
            : trimmed;
    }
}
