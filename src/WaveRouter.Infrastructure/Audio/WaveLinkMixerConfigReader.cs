using System.Text.Json;
using WaveRouter.Core.Abstractions;
using WaveRouter.Core.Models;

namespace WaveRouter.Infrastructure.Audio;

/// <summary>
/// Reads Wave Link's own Automixer state — see <see cref="WaveLinkPaths"/> for where. Parses whichever of
/// the two known schemas the resolved file uses; both are undocumented, private, and specific to the
/// installed Wave Link version, so any parse failure degrades to "nothing found", never a crash.
/// </summary>
public sealed class WaveLinkMixerConfigReader : IWaveLinkMixerConfigReader
{
    public IReadOnlyList<KnownAppAssignment> ReadKnownAssignments()
    {
        try
        {
            var path = WaveLinkPaths.ResolveConfigFilePath();
            if (path is null)
            {
                return [];
            }

            // Wave Link keeps this file open while it runs — File.ReadAllBytes's default sharing collides
            // with that and throws IOException ("used by another process"); explicit ReadWrite sharing
            // reads it fine concurrently.
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var document = JsonDocument.Parse(stream);
            return path.EndsWith("Settings.json", StringComparison.OrdinalIgnoreCase)
                ? ReadMsixSchema(document.RootElement)
                : ReadLegacyRoamingSchema(document.RootElement);
        }
        catch (Exception)
        {
            return [];
        }
    }

    /// <summary>
    /// Current (MSIX) schema, e.g. `LocalState/Settings.json`. Structure observed empirically (Wave Link
    /// 3.0, Aug 2026): `MixerConfiguration.InputSettings` is a dictionary keyed by internal device/channel
    /// ID; each entry's `DeviceSettings.DeviceName` gives the track's display name (e.g. "Game"), and its
    /// `AudioAppConfigurations[]` lists every app ever assigned there — including ones not currently
    /// running, unlike the live mixer state alone. Only entries with a non-null `ProcessName` are usable:
    /// many (apps added by browsing to a folder rather than being auto-detected while running) only have
    /// a `DisplayName` and an install path, with no way to derive a `Process.ProcessName` that will
    /// actually match a live session later.
    /// </summary>
    private static List<KnownAppAssignment> ReadMsixSchema(JsonElement root)
    {
        var assignments = new List<KnownAppAssignment>();
        if (!root.TryGetProperty("MixerConfiguration", out var mixerConfig) ||
            !mixerConfig.TryGetProperty("InputSettings", out var inputSettings))
        {
            return assignments;
        }

        foreach (var entry in inputSettings.EnumerateObject())
        {
            var input = entry.Value;
            if (!input.TryGetProperty("DeviceSettings", out var deviceSettings) ||
                !deviceSettings.TryGetProperty("DeviceName", out var deviceNameProp) ||
                deviceNameProp.GetString() is not { Length: > 0 } trackName)
            {
                continue;
            }

            if (!input.TryGetProperty("AudioAppConfigurations", out var apps))
            {
                continue;
            }

            foreach (var app in apps.EnumerateArray())
            {
                if (app.TryGetProperty("ProcessName", out var processNameProp) &&
                    processNameProp.ValueKind == JsonValueKind.String &&
                    processNameProp.GetString() is { Length: > 0 } processName)
                {
                    assignments.Add(new KnownAppAssignment(processName, trackName));
                }
            }
        }

        return assignments;
    }

    /// <summary>
    /// Legacy (pre-MSIX) schema, e.g. `%AppData%/Elgato/WaveLink/MixerConfiguration.json`. Only reflects
    /// apps active in Wave Link's current/recent mixer session, not its full app history — kept as a
    /// fallback for installs that never migrated to the MSIX schema above.
    /// </summary>
    private static List<KnownAppAssignment> ReadLegacyRoamingSchema(JsonElement root)
    {
        var assignments = new List<KnownAppAssignment>();
        if (!root.TryGetProperty("configuration", out var configuration) ||
            !configuration.TryGetProperty("inputs", out var inputs))
        {
            return assignments;
        }

        foreach (var trackBucket in inputs.EnumerateArray())
        {
            var isTrackBucket = trackBucket.TryGetProperty("nodeType", out var nodeType) &&
                nodeType.ValueKind == JsonValueKind.Number && nodeType.GetInt32() == 5;
            if (!isTrackBucket || !trackBucket.TryGetProperty("userProvidedName", out var trackNameProp) ||
                trackNameProp.GetString() is not { Length: > 0 } trackName)
            {
                continue;
            }

            if (!trackBucket.TryGetProperty("inputs", out var apps))
            {
                continue;
            }

            foreach (var app in apps.EnumerateArray())
            {
                if (TryGetExecutableNameFromIdentifier(app) is { } executableName)
                {
                    assignments.Add(new KnownAppAssignment(executableName, trackName));
                }
            }
        }

        return assignments;
    }

    private static string? TryGetExecutableNameFromIdentifier(JsonElement app)
    {
        if (!app.TryGetProperty("isDesktopApp", out var isDesktopApp) || isDesktopApp.ValueKind != JsonValueKind.True)
        {
            return null; // UWP/Store apps don't map cleanly to Process.ProcessName — skip them.
        }

        if (!app.TryGetProperty("identifier", out var identifierProp) || identifierProp.GetString() is not { } identifier)
        {
            return null;
        }

        var pathWithoutPid = identifier.Split('|')[0];
        var fileName = Path.GetFileNameWithoutExtension(pathWithoutPid);
        return string.IsNullOrWhiteSpace(fileName) ? null : fileName;
    }
}
