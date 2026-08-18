using WaveRouter.Core.Models;

namespace WaveRouter.Core.Abstractions;

public interface IAudioSessionWatcher : IDisposable
{
    /// <summary>Raised when a session starts producing audio for the first time since watching began —
    /// either a session created after <see cref="Start"/>, or one that existed but was silent at
    /// <see cref="Start"/> and only later became active. This is "an app just started making sound":
    /// worth prompting the user about if no rule matches.</summary>
    event EventHandler<AudioSessionInfo>? NewSessionDetected;

    /// <summary>Raised once per already-active session found during <see cref="Start"/>'s initial scan —
    /// apps that were already playing before WaveRouter opened. A matching rule should still be
    /// (re-)applied, but this should never trigger an unconfigured-app prompt: the user didn't just
    /// launch it, and popping up a dialog for everything already open on the machine would be spam.</summary>
    event EventHandler<AudioSessionInfo>? ExistingActiveSessionDetected;

    void Start();
}
