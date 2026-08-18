using NAudio.CoreAudioApi;
using WaveRouter.Core.Abstractions;

namespace WaveRouter.Infrastructure.Audio;

/// <summary>
/// Lists Wave Link's tracks without reading its (undocumented, unlocated) config file: Wave Link exposes
/// each track as a regular Windows playback device named e.g. "Game (Elgato Virtual Audio)" — this just
/// enumerates active render devices and keeps the Elgato ones, stripping the vendor suffix for display.
/// The short name (e.g. "Game") is what gets stored as a Rule's TrackName — PolicyConfigAudioRouter already
/// matches it back to the full device name via a substring match, so no extra mapping is needed either way.
/// </summary>
public sealed class WaveLinkTrackProvider : ITrackProvider
{
    public IReadOnlyList<string> GetAvailableTracks()
    {
        using var enumerator = new MMDeviceEnumerator();
        var tracks = new List<string>();

        foreach (var device in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
        {
            if (WaveLinkDevices.TryGetTrackName(device.FriendlyName) is { } track)
            {
                tracks.Add(track);
            }
        }

        return tracks;
    }
}
