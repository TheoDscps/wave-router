using NAudio.CoreAudioApi;
using WaveRouter.Core.Abstractions;
using WaveRouter.Core.Models;
using WaveRouter.Infrastructure.Audio.PolicyConfig;

namespace WaveRouter.Infrastructure.Audio;

/// <summary>
/// Sets a process's default audio output device via the same undocumented mechanism Windows itself
/// uses for "App volume and device preferences" (Settings → System → Sound). There is no public,
/// documented API for this — this is a port of the approach used by EarTrumpet
/// (https://github.com/File-New-Project/EarTrumpet, MIT), a widely-used, actively maintained,
/// non-elevated tray app that does the exact same thing, which is why no admin rights are requested here.
/// </summary>
public sealed class PolicyConfigAudioRouter : IAudioRouter
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

            var wrappedDeviceId = $"{MMDeviceApiToken}{device.ID}{DeviceInterfaceAudioRender}";
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
