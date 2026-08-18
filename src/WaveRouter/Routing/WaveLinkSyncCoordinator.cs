using System.IO;
using WaveRouter.ViewModels;

namespace WaveRouter.Routing;

/// <summary>
/// Watches Wave Link's own config file (%AppData%/Elgato/WaveLink/MixerConfiguration.json) and silently
/// syncs any new app↔track assignments as Wave Link's Automixer learns them — a live sync instead of a
/// one-off manual import. No prompt: the user already made this choice inside Wave Link itself.
/// Debounced, since a single edit in Wave Link can trigger several rapid file-write events.
/// </summary>
public sealed class WaveLinkSyncCoordinator : IDisposable
{
    private const int DebounceMilliseconds = 800;

    private readonly RuleListViewModel _ruleList;
    private readonly FileSystemWatcher? _watcher;
    private readonly System.Threading.Timer _debounceTimer;

    public WaveLinkSyncCoordinator(RuleListViewModel ruleList)
    {
        _ruleList = ruleList;
        _debounceTimer = new System.Threading.Timer(_ => OnDebouncedChange(), null, Timeout.Infinite, Timeout.Infinite);

        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Elgato", "WaveLink");

        if (!Directory.Exists(directory))
        {
            // Wave Link has never run on this machine (or isn't installed) — nothing to watch.
            return;
        }

        _watcher = new FileSystemWatcher(directory, "MixerConfiguration.json")
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
        };
        _watcher.Changed += (_, _) => ScheduleSync();
        _watcher.Created += (_, _) => ScheduleSync();
        _watcher.EnableRaisingEvents = true;
    }

    private void ScheduleSync() => _debounceTimer.Change(DebounceMilliseconds, Timeout.Infinite);

    private void OnDebouncedChange()
    {
        // FileSystemWatcher callbacks run on a ThreadPool thread — rule list mutations need the UI thread.
        System.Windows.Application.Current?.Dispatcher.BeginInvoke(async () => await _ruleList.SyncFromWaveLinkAsync());
    }

    public void Dispose()
    {
        _watcher?.Dispose();
        _debounceTimer.Dispose();
    }
}
