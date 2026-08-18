using System.Runtime.InteropServices;
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
/// </summary>
public sealed class AudioSessionWatcher : IAudioSessionWatcher
{
    private readonly MMDeviceEnumerator _enumerator = new();
    private readonly List<MMDevice> _devices = [];
    private readonly HashSet<int> _notifiedProcessIds = [];
    private readonly Dictionary<PendingSessionHandler, AudioSessionControl> _pendingSessions = [];

    public event EventHandler<AudioSessionInfo>? NewSessionDetected;
    public event EventHandler<AudioSessionInfo>? ExistingActiveSessionDetected;

    public void Start()
    {
        foreach (var device in _enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
        {
            _devices.Add(device);
            device.AudioSessionManager.OnSessionCreated += OnSessionCreated;

            var sessions = device.AudioSessionManager.Sessions;
            for (var i = 0; i < sessions.Count; i++)
            {
                TrackSession(sessions[i], isInitialScan: true);
            }
        }
    }

    private void OnSessionCreated(object sender, IAudioSessionControl newSession)
    {
        if (newSession is AudioSessionControl session)
        {
            TrackSession(session, isInitialScan: false);
        }
    }

    private void TrackSession(AudioSessionControl session, bool isInitialScan)
    {
        try
        {
            // pid 0 is the generic "system sounds" placeholder session every device carries — not a real app.
            if ((int)session.GetProcessID == 0)
            {
                return;
            }

            if (session.State == AudioSessionState.AudioSessionStateActive)
            {
                RaiseIfNotAlreadyNotified(session, isInitialScan);
                return;
            }

            // Not producing sound yet — watch for it to become active instead of reporting it now.
            var handler = new PendingSessionHandler(this);
            _pendingSessions[handler] = session;
            session.RegisterEventClient(handler);
        }
        catch (COMException)
        {
            // Session died between enumeration and inspection — ignore, it'll show up again if relevant.
        }
    }

    private void OnPendingSessionStateChanged(PendingSessionHandler handler, AudioSessionState state)
    {
        if (state != AudioSessionState.AudioSessionStateActive)
        {
            return;
        }

        if (!_pendingSessions.Remove(handler, out var session))
        {
            return;
        }

        try
        {
            session.UnRegisterEventClient(handler);

            // Whether this session was found at Start() or created later, it was SILENT until just now —
            // that's always "just started making sound", never "already playing before WaveRouter opened".
            RaiseIfNotAlreadyNotified(session, isInitialScan: false);
        }
        catch (COMException)
        {
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
        foreach (var (handler, session) in _pendingSessions)
        {
            try
            {
                session.UnRegisterEventClient(handler);
            }
            catch (COMException)
            {
            }
        }

        _pendingSessions.Clear();

        foreach (var device in _devices)
        {
            device.AudioSessionManager.OnSessionCreated -= OnSessionCreated;
            device.Dispose();
        }

        _enumerator.Dispose();
    }

    /// <summary>One instance per not-yet-active session being watched — forwards only the state-changed
    /// callback the watcher cares about.</summary>
    private sealed class PendingSessionHandler(AudioSessionWatcher owner) : IAudioSessionEventsHandler
    {
        public void OnStateChanged(AudioSessionState state) => owner.OnPendingSessionStateChanged(this, state);

        public void OnVolumeChanged(float volume, bool isMuted)
        {
        }

        public void OnDisplayNameChanged(string displayName)
        {
        }

        public void OnIconPathChanged(string iconPath)
        {
        }

        public void OnChannelVolumeChanged(uint channelCount, IntPtr newVolumes, uint channelIndex)
        {
        }

        public void OnGroupingParamChanged(ref Guid groupingId)
        {
        }

        public void OnSessionDisconnected(AudioSessionDisconnectReason disconnectReason)
        {
        }
    }
}
