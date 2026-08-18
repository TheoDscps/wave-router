using WaveRouter.Core.History;
using WaveRouter.Core.Models;

namespace WaveRouter.Core.Tests.History;

public class RoutingHistoryTests
{
    private static RoutingHistoryEntry Entry(string name) =>
        new(DateTime.Now, name, name.ToLowerInvariant(), "Game", true, null);

    [Fact]
    public void Record_InsertsNewestFirst()
    {
        var history = new RoutingHistory();

        history.Record(Entry("First"));
        history.Record(Entry("Second"));

        Assert.Equal("Second", history.Entries[0].DisplayName);
        Assert.Equal("First", history.Entries[1].DisplayName);
    }

    [Fact]
    public void Record_RaisesEntryAdded_WithTheRecordedEntry()
    {
        var history = new RoutingHistory();
        RoutingHistoryEntry? raised = null;
        history.EntryAdded += (_, entry) => raised = entry;

        var recorded = Entry("Game.exe");
        history.Record(recorded);

        Assert.Equal(recorded, raised);
    }

    [Fact]
    public void Record_CapsAt200Entries_DroppingTheOldest()
    {
        var history = new RoutingHistory();

        for (var i = 0; i < 250; i++)
        {
            history.Record(Entry($"App{i}"));
        }

        Assert.Equal(200, history.Entries.Count);
        Assert.Equal("App249", history.Entries[0].DisplayName);
        Assert.Equal("App50", history.Entries[^1].DisplayName);
    }
}
