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
///
/// The device list isn't a one-time snapshot: an <see cref="IMMNotificationClient"/> keeps it in sync with
/// devices being plugged in, unplugged, enabled or disabled after <see cref="Start"/> — e.g. switching
/// audio output, or a headset reconnecting. Without this, a session created on a device that appeared
/// after <see cref="Start"/> would silently go unnoticed: no rule match, no unknown-app prompt, nothing
/// (confirmed as the actual cause of a real missed detection, not just a theoretical gap).
/// </summary>
public sealed class AudioSessionWatcher : IAudioSessionWatcher
{
    private readonly MMDeviceEnumerator _enumerator = new();
    private readonly Dictionary<string, MMDevice> _devices = [];
    private readonly HashSet<int> _notifiedProcessIds = [];
    private readonly Dictionary<PendingSessionHandler, AudioSessionControl> _pendingSessions = [];
    private readonly EndpointNotificationClient _notificationClient;

    public event EventHandler<AudioSessionInfo>? NewSessionDetected;
    public event EventHandler<AudioSessionInfo>? ExistingActiveSessionDetected;

    public AudioSessionWatcher()
    {
        _notificationClient = new EndpointNotificationClient(this);
    }

    public void Start()
    {
        foreach (var device in _enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
        {
            TrackDevice(device, isInitialScan: true);
        }

        _enumerator.RegisterEndpointNotificationCallback(_notificationClient);
    }

    /// <summary>Starts watching a render device — either found during <see cref="Start"/>'s initial scan,
    /// or one that appeared/became active afterwards. In the latter case any session already on it is
    /// treated as new (<paramref name="isInitialScan"/> false): the device wasn't there when watching
    /// began, so nothing on it could have been "already playing before WaveRouter opened".</summary>
    private void TrackDevice(MMDevice device, bool isInitialScan)
    {
        lock (_devices)
        {
            if (!_devices.TryAdd(device.ID, device))
            {
                device.Dispose();
                return;
            }
        }

        device.AudioSessionManager.OnSessionCreated += OnSessionCreated;

        var sessions = device.AudioSessionManager.Sessions;
        for (var i = 0; i < sessions.Count; i++)
        {
            TrackSession(sessions[i], isInitialScan);
        }
    }

    private void OnDeviceAvailable(string deviceId)
    {
        lock (_devices)
        {
            if (_devices.ContainsKey(deviceId))
            {
                return;
            }
        }

        MMDevice device;
        try
        {
            device = _enumerator.GetDevice(deviceId);
        }
        catch (COMException)
        {
            // Gone again already (e.g. a device that flickers add/remove) — nothing to track.
            return;
        }

        if (device.DataFlow != DataFlow.Render || device.State != DeviceState.Active)
        {
            device.Dispose();
            return;
        }

        TrackDevice(device, isInitialScan: false);
    }

    private void OnDeviceUnavailable(string deviceId)
    {
        lock (_devices)
        {
            if (!_devices.Remove(deviceId, out var device))
            {
                return;
            }

            device.AudioSessionManager.OnSessionCreated -= OnSessionCreated;
            device.Dispose();
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
        _enumerator.UnregisterEndpointNotificationCallback(_notificationClient);

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

        lock (_devices)
        {
            foreach (var device in _devices.Values)
            {
                device.AudioSessionManager.OnSessionCreated -= OnSessionCreated;
                device.Dispose();
            }

            _devices.Clear();
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

    /// <summary>Keeps the tracked device list in sync after <see cref="Start"/> — forwards only the
    /// add/remove/state-change callbacks the watcher cares about. Default-device changes are irrelevant
    /// here: every active render device is watched already, not just the current default.</summary>
    private sealed class EndpointNotificationClient(AudioSessionWatcher owner) : IMMNotificationClient
    {
        public void OnDeviceAdded(string pwstrDeviceId) => owner.OnDeviceAvailable(pwstrDeviceId);

        public void OnDeviceRemoved(string deviceId) => owner.OnDeviceUnavailable(deviceId);

        public void OnDeviceStateChanged(string deviceId, DeviceState newState)
        {
            if (newState == DeviceState.Active)
            {
                owner.OnDeviceAvailable(deviceId);
            }
            else
            {
                owner.OnDeviceUnavailable(deviceId);
            }
        }

        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
        {
        }

        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
        {
        }
    }
}
