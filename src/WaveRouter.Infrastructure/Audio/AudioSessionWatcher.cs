using System.Runtime.InteropServices;
using System.Threading;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using WaveRouter.Core.Abstractions;
using WaveRouter.Core.Models;

namespace WaveRouter.Infrastructure.Audio;

/// <summary>
/// Watches every active render device — not just the OS "default" one — for audio sessions that start
/// producing sound. Watching only the default device would miss most real sessions in a Wave Link setup:
/// apps get routed to their own per-app virtual device, which is rarely the current system default
/// (confirmed empirically — an app already routed to e.g. "Music (Elgato Virtual Audio)" has its session
/// there, never on "System").
///
/// Sessions already active when <see cref="Start"/> runs (apps the user already had open) raise
/// <see cref="ExistingActiveSessionDetected"/> — worth (re-)applying a matching rule to, but not worth
/// prompting about, since the user didn't just launch them. Everything else — a session created after
/// <see cref="Start"/>, or one that existed but was silent and only later becomes active — raises
/// <see cref="NewSessionDetected"/>: a genuine "an app just started making sound" event.
///
/// Detection is done by polling (<see cref="PollInterval"/>), not by <c>IAudioSessionManager</c>'s
/// <c>OnSessionCreated</c>/<c>IAudioSessionEvents</c> push notifications — those were the original
/// implementation, but proved unreliable on real hardware: confirmed via an isolated harness (a genuine
/// WASAPI render stream opened on an already-tracked, already-enumerated device, both with and without a
/// Win32 message pump running) that the callback simply never fires, even though the session itself
/// becomes properly Active and is visible to a fresh <c>AudioSessionManager.Sessions</c> enumeration.
/// This matches a known class of Windows Core Audio notification reliability issues (worse still with
/// virtual-audio drivers layered in front of the real endpoint, as with Wave Link/Voicemod here) — polling
/// re-enumerates devices and sessions from scratch every tick, so it can't silently miss anything the way
/// a dropped COM callback can. The device list is re-enumerated every tick too rather than cached, so a
/// device that appears/disappears after <see cref="Start"/> (headset reconnect, output switch) is picked
/// up for free, without needing a separate <c>IMMNotificationClient</c>.
/// </summary>
public sealed class AudioSessionWatcher : IAudioSessionWatcher
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);

    private readonly MMDeviceEnumerator _enumerator = new();
    private readonly HashSet<int> _notifiedProcessIds = [];
    private readonly Lock _pollLock = new();
    private Timer? _pollTimer;

    public event EventHandler<AudioSessionInfo>? NewSessionDetected;
    public event EventHandler<AudioSessionInfo>? ExistingActiveSessionDetected;

    public void Start()
    {
        Poll(isInitialScan: true);
        _pollTimer = new Timer(_ => Poll(isInitialScan: false), null, PollInterval, PollInterval);
    }

    private void Poll(bool isInitialScan)
    {
        // A tick firing while the previous one is still running (a slow/hung endpoint) skips rather than
        // overlapping — the next tick two seconds later covers the same ground.
        if (!_pollLock.TryEnter())
        {
            return;
        }

        try
        {
            foreach (var device in _enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                using (device)
                {
                    ScanDevice(device, isInitialScan);
                }
            }
        }
        finally
        {
            _pollLock.Exit();
        }
    }

    private void ScanDevice(MMDevice device, bool isInitialScan)
    {
        try
        {
            var sessions = device.AudioSessionManager.Sessions;
            for (var i = 0; i < sessions.Count; i++)
            {
                var session = sessions[i];

                // pid 0 is the generic "system sounds" placeholder session every device carries — not a real app.
                if ((int)session.GetProcessID == 0 || session.State != AudioSessionState.AudioSessionStateActive)
                {
                    continue;
                }

                RaiseIfNotAlreadyNotified(session, isInitialScan);
            }
        }
        catch (COMException)
        {
            // Device/session vanished mid-poll — ignore, it'll show up again next tick if still relevant.
        }
    }

    private void RaiseIfNotAlreadyNotified(AudioSessionControl session, bool isInitialScan)
    {
        try
        {
            var processId = (int)session.GetProcessID;
            if (!_notifiedProcessIds.Add(processId))
            {
                return;
            }

            var processName = ResolveProcessName(processId);
            var displayName = string.IsNullOrWhiteSpace(session.DisplayName)
                ? processName
                : session.DisplayName;
            var info = new AudioSessionInfo(processId, processName, displayName);

            if (isInitialScan)
            {
                ExistingActiveSessionDetected?.Invoke(this, info);
            }
            else
            {
                NewSessionDetected?.Invoke(this, info);
            }
        }
        catch (COMException)
        {
        }
    }

    private static string ResolveProcessName(int processId)
    {
        try
        {
            return System.Diagnostics.Process.GetProcessById(processId).ProcessName;
        }
        catch (ArgumentException)
        {
            return $"pid:{processId}";
        }
    }

    public void Dispose()
    {
        _pollTimer?.Dispose();
        _enumerator.Dispose();
    }
}
