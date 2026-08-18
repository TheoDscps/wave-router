using System.Text.Json;
using WaveRouter.Core.Abstractions;
using WaveRouter.Core.Models;

namespace WaveRouter.Infrastructure.Audio;

/// <summary>
/// Reads %AppData%/Elgato/WaveLink/MixerConfiguration.json — Wave Link's own Automixer state. This is an
/// undocumented, private file format specific to the installed Wave Link version; there is no public
/// schema for it. Structure observed empirically (Wave Link 3.0, Aug 2026):
/// configuration.inputs[] holds both physical devices (nodeType 4 mics, nodeType 1 aux inputs) and track
/// "buckets" (nodeType 5, e.g. "Wave Link Game") — each bucket's own nested inputs[] lists the apps
/// currently assigned to it, identified as "{full exe path}|{pid}" for regular desktop apps.
/// UWP/Store apps (isDesktopApp: false, e.g. Microsoft Teams) are skipped: their identifier doesn't map
/// cleanly to a Process.ProcessName the way a desktop exe's path does.
/// </summary>
public sealed class WaveLinkMixerConfigReader : IWaveLinkMixerConfigReader
{
    private const int TrackBucketNodeType = 5;

    public IReadOnlyList<KnownAppAssignment> ReadKnownAssignments()
    {
        try
        {
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Elgato", "WaveLink", "MixerConfiguration.json");

            if (!File.Exists(path))
            {
                return [];
            }

            using var document = JsonDocument.Parse(File.ReadAllBytes(path));
            var inputs = document.RootElement.GetProperty("configuration").GetProperty("inputs");

            var assignments = new List<KnownAppAssignment>();
            foreach (var trackBucket in inputs.EnumerateArray())
            {
                if (!IsTrackBucket(trackBucket) || !trackBucket.TryGetProperty("userProvidedName", out var trackNameProp))
                {
                    continue;
                }

                var trackName = trackNameProp.GetString();
                if (string.IsNullOrWhiteSpace(trackName) || !trackBucket.TryGetProperty("inputs", out var apps))
                {
                    continue;
                }

                foreach (var app in apps.EnumerateArray())
                {
                    if (TryGetExecutableName(app) is { } executableName)
                    {
                        assignments.Add(new KnownAppAssignment(executableName, trackName));
                    }
                }
            }

            return assignments;
        }
        catch (Exception)
        {
            // Any parse failure (schema change in a future Wave Link version, corrupted file) must
            // degrade to "nothing to import" — never crash the app over an undocumented file format.
            return [];
        }
    }

    private static bool IsTrackBucket(JsonElement input) =>
        input.TryGetProperty("nodeType", out var nodeType) && nodeType.ValueKind == JsonValueKind.Number && nodeType.GetInt32() == TrackBucketNodeType;

    private static string? TryGetExecutableName(JsonElement app)
    {
        if (!app.TryGetProperty("isDesktopApp", out var isDesktopApp) || isDesktopApp.ValueKind != JsonValueKind.True)
        {
            return null;
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
