using WaveRouter.Core.Models;
using WaveRouter.Core.Rules;

namespace WaveRouter.Core.Tests.Rules;

public class RuleMatcherTests
{
    [Theory]
    [InlineData("discord.exe", "discord")]
    [InlineData("Discord.EXE", "Discord")]
    [InlineData("discord", "discord")]
    [InlineData("  discord.exe  ", "discord")]
    public void NormalizeExecutableName_StripsExeSuffixCaseInsensitively(string input, string expected)
    {
        Assert.Equal(expected, RuleMatcher.NormalizeExecutableName(input));
    }

    [Fact]
    public void FindMatch_ReturnsRule_WhenExecutableMatchesExactly()
    {
        var rules = new List<Rule> { new("discord.exe", "Voice chat") };

        var match = RuleMatcher.FindMatch(rules, "discord");

        Assert.NotNull(match);
        Assert.Equal("Voice chat", match.TrackName);
    }

    [Fact]
    public void FindMatch_IsCaseInsensitive()
    {
        var rules = new List<Rule> { new("Discord.exe", "Voice chat") };

        var match = RuleMatcher.FindMatch(rules, "DISCORD");

        Assert.NotNull(match);
    }

    [Fact]
    public void FindMatch_ReturnsNull_WhenNoRuleMatches()
    {
        var rules = new List<Rule> { new("discord.exe", "Voice chat") };

        var match = RuleMatcher.FindMatch(rules, "chrome");

        Assert.Null(match);
    }

    [Fact]
    public void FindMatch_ReturnsNull_WhenRuleListIsEmpty()
    {
        Assert.Null(RuleMatcher.FindMatch([], "discord"));
    }

    [Theory]
    [InlineData("chrome*", "chrome")]
    [InlineData("chrome*", "chrome_beta")]
    [InlineData("chrome*", "CHROME_CANARY")]
    [InlineData("*helper", "gpu_helper")]
    [InlineData("game?", "game1")]
    public void FindMatch_SupportsWildcardPatterns(string pattern, string processName)
    {
        var rules = new List<Rule> { new(pattern, "Game") };

        var match = RuleMatcher.FindMatch(rules, processName);

        Assert.NotNull(match);
    }

    [Theory]
    [InlineData("chrome*", "opera")]
    [InlineData("game?", "game10")]
    [InlineData("game?", "game")]
    public void FindMatch_WildcardPattern_DoesNotMatchUnrelatedOrWrongLength(string pattern, string processName)
    {
        var rules = new List<Rule> { new(pattern, "Game") };

        Assert.Null(RuleMatcher.FindMatch(rules, processName));
    }

    [Fact]
    public void FindMatch_PlainPatternWithoutWildcard_StillRequiresExactMatch()
    {
        var rules = new List<Rule> { new("chrome.exe", "Browser") };

        Assert.Null(RuleMatcher.FindMatch(rules, "chrome_beta"));
    }

    [Fact]
    public void IsIgnored_SupportsWildcardPatterns()
    {
        var ignored = new List<string> { "obs*" };

        Assert.True(RuleMatcher.IsIgnored(ignored, "obs64"));
    }

    [Fact]
    public void IsIgnored_IsTrue_ForNormalizedMatch()
    {
        var ignored = new List<string> { "spotify.exe" };

        Assert.True(RuleMatcher.IsIgnored(ignored, "Spotify"));
    }

    [Fact]
    public void IsIgnored_IsFalse_WhenNotInList()
    {
        var ignored = new List<string> { "spotify.exe" };

        Assert.False(RuleMatcher.IsIgnored(ignored, "chrome"));
    }

    [Fact]
    public void IsSystemProcess_IsTrue_ForElgatoWaveLink()
    {
        Assert.True(RuleMatcher.IsSystemProcess("Elgato.WaveLink"));
    }

    [Fact]
    public void IsSystemProcess_IsFalse_ForRegularApp()
    {
        Assert.False(RuleMatcher.IsSystemProcess("chrome"));
    }
}
