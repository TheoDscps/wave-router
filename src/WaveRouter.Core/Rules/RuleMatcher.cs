using System.Text.RegularExpressions;
using WaveRouter.Core.Models;

namespace WaveRouter.Core.Rules;

/// <summary>Finds the rule matching a detected process. See docs/use-cases/automatic-routing-enforcement.md.</summary>
public static class RuleMatcher
{
    /// <summary>Elgato Wave Link's own process keeps audio streams open on multiple devices for its internal
    /// routing/monitoring, showing up as "active" even with nothing audible — prompting to route it to
    /// itself makes no sense, so it's excluded from the unsolicited new-app prompt by default. A rule can
    /// still be created for it manually if a user genuinely wants one.</summary>
    private static readonly string[] SystemProcessNames = ["Elgato.WaveLink"];

    public static bool IsSystemProcess(string processName) =>
        SystemProcessNames.Any(name => string.Equals(NormalizeExecutableName(name), NormalizeExecutableName(processName), StringComparison.OrdinalIgnoreCase));

    public static Rule? FindMatch(IReadOnlyList<Rule> rules, string processName)
    {
        var normalizedProcessName = NormalizeExecutableName(processName);
        foreach (var rule in rules)
        {
            if (Matches(rule.ExecutableName, normalizedProcessName))
            {
                return rule;
            }
        }

        return null;
    }

    public static bool IsIgnored(IReadOnlyList<string> ignoredExecutables, string processName)
    {
        var normalizedProcessName = NormalizeExecutableName(processName);
        foreach (var ignored in ignoredExecutables)
        {
            if (Matches(ignored, normalizedProcessName))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Rules are authored with a ".exe" suffix (e.g. "discord.exe") but <see cref="System.Diagnostics.Process.ProcessName"/>
    /// never includes it — strip it from both sides so "discord.exe" matches "discord".</summary>
    public static string NormalizeExecutableName(string executableOrProcessName)
    {
        var trimmed = executableOrProcessName.Trim();
        return trimmed.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
            ? trimmed[..^4]
            : trimmed;
    }

    /// <summary>True if the already-normalized <paramref name="normalizedProcessName"/> matches
    /// <paramref name="pattern"/> as authored in a rule or ignore entry. A pattern with no "*"/"?" falls
    /// back to a plain equality check — so every rule created before wildcard support existed keeps
    /// matching exactly as before. "*" matches any run of characters (e.g. "chrome*" covers browser
    /// helper sub-processes like "chrome_beta"), "?" matches exactly one.</summary>
    private static bool Matches(string pattern, string normalizedProcessName)
    {
        var normalizedPattern = NormalizeExecutableName(pattern);
        if (!ContainsWildcard(normalizedPattern))
        {
            return string.Equals(normalizedPattern, normalizedProcessName, StringComparison.OrdinalIgnoreCase);
        }

        var escaped = Regex.Escape(normalizedPattern).Replace(@"\*", ".*").Replace(@"\?", ".");
        return Regex.IsMatch(normalizedProcessName, $"^{escaped}$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private static bool ContainsWildcard(string pattern) => pattern.Contains('*') || pattern.Contains('?');
}
