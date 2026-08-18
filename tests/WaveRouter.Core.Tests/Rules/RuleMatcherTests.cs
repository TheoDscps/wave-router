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
