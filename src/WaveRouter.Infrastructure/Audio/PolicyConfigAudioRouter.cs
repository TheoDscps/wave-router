using NAudio.CoreAudioApi;
using WaveRouter.Core.Abstractions;
using WaveRouter.Core.Models;
using WaveRouter.Infrastructure.Audio.PolicyConfig;

namespace WaveRouter.Infrastructure.Audio;

/// <summary>
/// Sets (and reads) a process's default audio output device via the same undocumented mechanism Windows
/// itself uses for "App volume and device preferences" (Settings → System → Sound). There is no public,
/// documented API for this — this is a port of the approach used by EarTrumpet
/// (https://github.com/File-New-Project/EarTrumpet, MIT), a widely-used, actively maintained,
/// non-elevated tray app that does the exact same thing, which is why no admin rights are requested here.
/// </summary>
public sealed class PolicyConfigAudioRouter : IAudioRouter, IExistingRoutingScanner
{
    private const string MMDeviceApiToken = @"\\?\SWD#MMDEVAPI#";
    private const string DeviceInterfaceAudioRender = "#{e6327cad-dcec-4949-ae8a-991e976a79d2}";

    private readonly Lazy<IAudioPolicyConfigFactory> _policyConfig = new(AudioPolicyConfigFactorySelector.Create);

    public RoutingResult ApplyRule(int processId, string trackName)
    {
        try
        {
            using var enumerator = new MMDeviceEnumerator();
            var device = FindRenderDeviceByName(enumerator, trackName);
            if (device is null)
            {
                return RoutingResult.Failed($"No active output device matching \"{trackName}\" was found.");
            }

            var wrappedDeviceId = WrapDeviceId(device.ID);
            Combase.WindowsCreateString(wrappedDeviceId, (uint)wrappedDeviceId.Length, out var hstring);

            var factory = _policyConfig.Value;
            var multimediaResult = factory.SetPersistedDefaultAudioEndpoint(
                (uint)processId, DataFlow.Render, Role.Multimedia, hstring);
            var consoleResult = factory.SetPersistedDefaultAudioEndpoint(
                (uint)processId, DataFlow.Render, Role.Console, hstring);

            return multimediaResult == HRESULT.S_OK && consoleResult == HRESULT.S_OK
                ? RoutingResult.Ok()
                : RoutingResult.Failed($"Windows rejected the routing request (HRESULT {multimediaResult}/{consoleResult}).");
        }
        catch (Exception ex)
        {
            // This wraps an undocumented, OS-version-sensitive WinRT API — any failure here (missing
            // interface, marshaling error, unsupported OS build) must not crash the app.
            return RoutingResult.Failed($"Routing failed: {ex.Message}");
        }
    }

    /// <summary>Used to import assignments the user made manually (via Windows Settings) before ever
    /// using WaveRouter — see docs/use-cases/read-wave-link-tracks.md.</summary>
    public string? GetExistingTrackAssignment(int processId)
    {
        try
        {
            var factory = _policyConfig.Value;
            var result = factory.GetPersistedDefaultAudioEndpoint(
                (uint)processId, DataFlow.Render, Role.Multimedia, out var deviceIdHString);
            var wrappedDeviceId = Combase.ReadAndDeleteHString(deviceIdHString);

            if (result != HRESULT.S_OK || string.IsNullOrEmpty(wrappedDeviceId))
            {
                return null;
            }

            using var enumerator = new MMDeviceEnumerator();
            var device = enumerator.GetDevice(UnwrapDeviceId(wrappedDeviceId));
            return WaveLinkDevices.TryGetTrackName(device.FriendlyName);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static string WrapDeviceId(string rawDeviceId) => $"{MMDeviceApiToken}{rawDeviceId}{DeviceInterfaceAudioRender}";

    private static string UnwrapDeviceId(string wrappedDeviceId)
    {
        var id = wrappedDeviceId;
        if (id.StartsWith(MMDeviceApiToken, StringComparison.OrdinalIgnoreCase))
        {
            id = id[MMDeviceApiToken.Length..];
        }

        if (id.EndsWith(DeviceInterfaceAudioRender, StringComparison.OrdinalIgnoreCase))
        {
            id = id[..^DeviceInterfaceAudioRender.Length];
        }

        return id;
    }

    private static MMDevice? FindRenderDeviceByName(MMDeviceEnumerator enumerator, string trackName)
    {
        var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
        foreach (var device in devices)
        {
            if (device.FriendlyName.Contains(trackName, StringComparison.OrdinalIgnoreCase))
            {
                return device;
            }
        }

        return null;
    }
}
