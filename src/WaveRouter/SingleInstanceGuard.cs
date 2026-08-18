namespace WaveRouter;

/// <summary>
/// Ensures only one WaveRouter instance runs at a time — a second watcher would double-apply routing.
/// The first instance owns a named mutex for its whole lifetime; any later launch sees it's already
/// taken, signals the first instance to show its window, and exits immediately instead of starting up
/// normally. See docs/use-cases/background-tray-execution.md.
/// </summary>
public sealed class SingleInstanceGuard : IDisposable
{
    private const string MutexName = "Local\\WaveRouter-SingleInstance";
    private const string ShowSignalName = "Local\\WaveRouter-ShowMainWindow";

    private readonly Mutex _mutex;
    private readonly EventWaitHandle _showSignal;
    private CancellationTokenSource? _listenerCts;

    /// <summary>False if another instance already owns the mutex.</summary>
    public bool IsFirstInstance { get; }

    public SingleInstanceGuard()
    {
        _mutex = new Mutex(initiallyOwned: true, MutexName, out var createdNew);
        IsFirstInstance = createdNew;
        _showSignal = new EventWaitHandle(false, EventResetMode.AutoReset, ShowSignalName);
    }

    /// <summary>First instance only: invokes <paramref name="onShowRequested"/> on the UI thread whenever
    /// a later launch signals it via <see cref="SignalExistingInstance"/>.</summary>
    public void ListenForActivationRequests(Action onShowRequested)
    {
        _listenerCts = new CancellationTokenSource();
        var token = _listenerCts.Token;
        var dispatcher = System.Windows.Application.Current.Dispatcher;

        Task.Run(() =>
        {
            while (!token.IsCancellationRequested)
            {
                if (_showSignal.WaitOne(500) && !token.IsCancellationRequested)
                {
                    dispatcher.Invoke(onShowRequested);
                }
            }
        }, token);
    }

    /// <summary>Second (or later) instance only: asks the running instance to show its window.</summary>
    public void SignalExistingInstance() => _showSignal.Set();

    public void Dispose()
    {
        _listenerCts?.Cancel();
        if (IsFirstInstance)
        {
            _mutex.ReleaseMutex();
        }

        _mutex.Dispose();
        _showSignal.Dispose();
    }
}
