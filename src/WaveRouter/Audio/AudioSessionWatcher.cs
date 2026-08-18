using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

namespace WaveRouter.Audio;

/// <summary>
/// Watches the default render device for newly created audio sessions (i.e. an app that just
/// started playing audio) and raises <see cref="NewSessionDetected"/> for each one.
/// </summary>
public sealed class AudioSessionWatcher : IDisposable
{
    private readonly MMDeviceEnumerator _enumerator = new();
    private MMDevice? _device;

    public event EventHandler<AudioSessionInfo>? NewSessionDetected;

    public void Start()
    {
        _device = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        _device.AudioSessionManager.OnSessionCreated += OnSessionCreated;

        // Surface sessions that already exist at startup too.
        var sessions = _device.AudioSessionManager.Sessions;
        for (var i = 0; i < sessions.Count; i++)
        {
            TryRaise(sessions[i]);
        }
    }

    private void OnSessionCreated(object sender, IAudioSessionControl newSession)
    {
        if (newSession is AudioSessionControl session)
        {
            TryRaise(session);
        }
    }

    private void TryRaise(AudioSessionControl session)
    {
        try
        {
            var processId = (int)session.GetProcessID;
            var processName = ResolveProcessName(processId);
            var displayName = string.IsNullOrWhiteSpace(session.DisplayName)
                ? processName
                : session.DisplayName;

            NewSessionDetected?.Invoke(this, new AudioSessionInfo(processId, processName, displayName));
        }
        catch (System.Runtime.InteropServices.COMException)
        {
            // Session died between enumeration and inspection — ignore, it will show up again if relevant.
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
        if (_device is not null)
        {
            _device.AudioSessionManager.OnSessionCreated -= OnSessionCreated;
        }

        _device?.Dispose();
        _enumerator.Dispose();
    }
}
