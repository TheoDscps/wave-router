using WaveRouter.Core.Models;

namespace WaveRouter.Core.History;

/// <summary>In-memory log of routing decisions, newest first, capped so a long-running session doesn't
/// grow this unbounded. No Windows dependency — the WPF layer subscribes to <see cref="EntryAdded"/> to
/// keep a bound UI collection in sync.</summary>
public sealed class RoutingHistory
{
    private const int MaxEntries = 200;
    private readonly List<RoutingHistoryEntry> _entries = [];

    public event EventHandler<RoutingHistoryEntry>? EntryAdded;

    public IReadOnlyList<RoutingHistoryEntry> Entries => _entries;

    public void Record(RoutingHistoryEntry entry)
    {
        _entries.Insert(0, entry);
        if (_entries.Count > MaxEntries)
        {
            _entries.RemoveAt(_entries.Count - 1);
        }

        EntryAdded?.Invoke(this, entry);
    }
}
