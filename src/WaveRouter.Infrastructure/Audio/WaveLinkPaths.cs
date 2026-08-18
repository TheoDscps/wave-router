namespace WaveRouter.Infrastructure.Audio;

/// <summary>
/// Locates Wave Link's live config file. There are two known locations/schemas, discovered by directly
/// inspecting a real installation (Aug 2026) — neither is documented:
/// - The current one: the MSIX-packaged app's `Settings.json`, under
///   `%LocalAppData%/Packages/Elgato.WaveLink_&lt;publisher-hash&gt;/LocalState/`. The publisher-hash
///   suffix is matched with a wildcard rather than hardcoded, since it's certificate-derived and not
///   guaranteed identical across installs.
/// - A legacy one: `%AppData%/Elgato/WaveLink/MixerConfiguration.json`, from an older non-MSIX Wave Link
///   version — found stale (untouched for 5 months) on the machine this was built against, so it's kept
///   only as a fallback for installs that never migrated.
/// Both <see cref="WaveLinkMixerConfigReader"/> and <see cref="WaveRouter.Routing.WaveLinkSyncCoordinator"/>
/// (the file watcher) must resolve to the same path, or the watcher ends up watching a file nothing writes
/// to — which is exactly the bug this class fixes.
/// </summary>
public static class WaveLinkPaths
{
    public static string? ResolveConfigFilePath()
    {
        var msixPath = TryResolveMsixSettingsPath();
        if (msixPath is not null && File.Exists(msixPath))
        {
            return msixPath;
        }

        var legacyPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Elgato", "WaveLink", "MixerConfiguration.json");
        return File.Exists(legacyPath) ? legacyPath : null;
    }

    private static string? TryResolveMsixSettingsPath()
    {
        var packagesRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Packages");

        if (!Directory.Exists(packagesRoot))
        {
            return null;
        }

        var packageDir = Directory.GetDirectories(packagesRoot, "Elgato.WaveLink_*").FirstOrDefault();
        return packageDir is null ? null : Path.Combine(packageDir, "LocalState", "Settings.json");
    }
}
