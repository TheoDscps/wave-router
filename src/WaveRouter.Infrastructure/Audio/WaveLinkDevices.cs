namespace WaveRouter.Infrastructure.Audio;

/// <summary>Wave Link registers each of its tracks as a real Windows playback device named
/// "{track} (Elgato Virtual Audio)" — shared by <see cref="WaveLinkTrackProvider"/> and
/// <see cref="PolicyConfigAudioRouter"/>'s existing-assignment lookup.</summary>
internal static class WaveLinkDevices
{
    public const string VendorSuffix = " (Elgato Virtual Audio)";

    public static string? TryGetTrackName(string deviceFriendlyName) =>
        deviceFriendlyName.EndsWith(VendorSuffix, StringComparison.OrdinalIgnoreCase)
            ? deviceFriendlyName[..^VendorSuffix.Length]
            : null;
}
