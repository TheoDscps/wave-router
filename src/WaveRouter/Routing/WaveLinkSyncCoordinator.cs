using System.IO;
using WaveRouter.Infrastructure.Audio;
using WaveRouter.ViewModels;

namespace WaveRouter.Routing;

/// <summary>
/// Watches Wave Link's own config file (see <see cref="WaveLinkPaths"/> for where — resolved once at
/// startup, same as <see cref="WaveLinkMixerConfigReader"/>, so both always agree on the same file) and
/// silently syncs any new app↔track assignments as Wave Link's Automixer learns them — a live sync
/// instead of a one-off manual import. No prompt: the user already made this choice inside Wave Link
/// itself. Debounced, since a single edit in Wave Link can trigger several rapid file-write events
/// (confirmed empirically — 6 change events observed for 3 writes in an isolated test).
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

        var configPath = WaveLinkPaths.ResolveConfigFilePath();
        if (configPath is null)
        {
            // Wave Link has never run on this machine (or isn't installed) — nothing to watch yet.
            return;
        }

        var directory = Path.GetDirectoryName(configPath)!;
        var fileName = Path.GetFileName(configPath);

        _watcher = new FileSystemWatcher(directory, fileName)
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
