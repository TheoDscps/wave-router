using WaveRouter.Core.Models;

namespace WaveRouter.Core.Abstractions;

public interface IAudioSessionWatcher : IDisposable
{
    /// <summary>Raised for every audio session that starts while watching, and for sessions already active when <see cref="Start"/> runs.</summary>
    event EventHandler<AudioSessionInfo>? NewSessionDetected;

    void Start();
}
