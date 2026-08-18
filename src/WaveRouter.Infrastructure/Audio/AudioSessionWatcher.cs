using System.Runtime.InteropServices;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using WaveRouter.Core.Abstractions;
using WaveRouter.Core.Models;

namespace WaveRouter.Infrastructure.Audio;

/// <summary>
/// Watches every active render device — not just the OS "default" one — for audio sessions that start
/// producing sound, and raises <see cref="NewSessionDetected"/> once per process. Watching only the
/// default device would miss most real sessions in a Wave Link setup: apps get routed to their own
/// per-app virtual device, which is rarely the current system default (confirmed empirically — an app
/// already routed to e.g. "Music (Elgato Virtual Audio)" has its session there, never on "System").
/// A session can exist before it's actually playing anything (e.g. a media player opened but paused) —
/// those are tracked via <see cref="IAudioSessionEventsHandler.OnStateChanged"/> until they go Active,
/// rather than raised immediately, so the app isn't reported as "detected" before it makes any sound.
/// </summary>
public sealed class AudioSessionWatcher : IAudioSessionWatcher
{
    private readonly MMDeviceEnumerator _enumerator = new();
    private readonly List<MMDevice> _devices = [];
    private readonly HashSet<int> _notifiedProcessIds = [];
    private readonly Dictionary<PendingSessionHandler, AudioSessionControl> _pendingSessions = [];

    public event EventHandler<AudioSessionInfo>? NewSessionDetected;

    public void Start()
    {
        foreach (var device in _enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
        {
            _devices.Add(device);
            device.AudioSessionManager.OnSessionCreated += OnSessionCreated;

            var sessions = device.AudioSessionManager.Sessions;
            for (var i = 0; i < sessions.Count; i++)
            {
                TrackSession(sessions[i]);
            }
        }
    }

    private void OnSessionCreated(object sender, IAudioSessionControl newSession)
    {
        if (newSession is AudioSessionControl session)
        {
            TrackSession(session);
        }
    }

    private void TrackSession(AudioSessionControl session)
    {
        try
        {
            if (session.State == AudioSessionState.AudioSessionStateActive)
            {
                RaiseIfNotAlreadyNotified(session);
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
            RaiseIfNotAlreadyNotified(session);
        }
        catch (COMException)
        {
        }
    }

    private void RaiseIfNotAlreadyNotified(AudioSessionControl session)
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

            NewSessionDetected?.Invoke(this, new AudioSessionInfo(processId, processName, displayName));
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
