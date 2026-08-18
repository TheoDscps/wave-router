using System.Collections.ObjectModel;
using WaveRouter.Core.History;
using WaveRouter.Core.Models;
using WaveRouter.Mvvm;
using Application = System.Windows.Application;

namespace WaveRouter.ViewModels;

/// <summary>Backs <see cref="HistoryWindow"/>. <see cref="RoutingHistory"/> itself has no Windows/WPF
/// dependency, so this class is the bridge: it seeds from whatever was already recorded before the window
/// was ever opened, then mirrors new entries live via <see cref="RoutingHistory.EntryAdded"/> — marshaled
/// onto the UI thread since that event can fire from the NAudio session-callback thread (see
/// <see cref="Routing.RuleMatchCoordinator"/>), not the dispatcher thread the bound collection needs.</summary>
public sealed class HistoryViewModel : ObservableObject
{
    private readonly RoutingHistory _history;

    public HistoryViewModel(RoutingHistory history)
    {
        _history = history;
        Entries = new ObservableCollection<RoutingHistoryEntry>(history.Entries);
        _history.EntryAdded += OnEntryAdded;
    }

    public ObservableCollection<RoutingHistoryEntry> Entries { get; }

    private void OnEntryAdded(object? sender, RoutingHistoryEntry entry) =>
        Application.Current.Dispatcher.Invoke(() =>
        {
            Entries.Insert(0, entry);

            // Mirror RoutingHistory's own cap so this bound collection can't outgrow it — otherwise a
            // long-running session (this is a tray app, meant to stay open indefinitely) would keep this
            // list growing forever even though the source it mirrors is capped.
            while (Entries.Count > _history.Entries.Count)
            {
                Entries.RemoveAt(Entries.Count - 1);
            }
        });
}
