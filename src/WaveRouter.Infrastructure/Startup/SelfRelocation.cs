using System.Diagnostics;

namespace WaveRouter.Infrastructure.Startup;

/// <summary>Ensures WaveRouter always runs from a stable per-user install path
/// (%LocalAppData%\WaveRouter\WaveRouter.exe) rather than wherever its release zip happened to be extracted
/// (e.g. Downloads) — extracting a newer version to a different folder would otherwise silently break the
/// Windows "Run" startup key registered against the old, now-missing path. On mismatch, this copies the
/// running exe to the stable path, refreshes the startup registration if it was enabled, relaunches from
/// there, and tells the caller to shut down immediately so the relaunched instance can take over.</summary>
public static class SelfRelocation
{
    public static string InstallDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WaveRouter");

    public static bool RelocateAndRelaunchIfNeeded()
    {
#if DEBUG
        // Dev builds run from bin\Debug\... — relocating there would copy the debug exe into the real
        // install path and fight with any release build already using it.
        return false;
#else
        try
        {
            var currentPath = Environment.ProcessPath;
            if (string.IsNullOrEmpty(currentPath))
            {
                return false;
            }

            var targetPath = Path.Combine(InstallDirectory, Path.GetFileName(currentPath));
            if (string.Equals(currentPath, targetPath, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            Directory.CreateDirectory(InstallDirectory);
            File.Copy(currentPath, targetPath, overwrite: true);

            var startupRegistration = new WindowsStartupRegistration();
            if (startupRegistration.IsEnabled())
            {
                // Re-point the Run key at the stable path directly — the process still running here is the
                // OLD exe, so Environment.ProcessPath-based SetEnabled(true) would just re-write the old path.
                startupRegistration.SetEnabledForExePath(targetPath);
            }

            Process.Start(new ProcessStartInfo(targetPath) { UseShellExecute = true });
            return true;
        }
        catch (Exception)
        {
            // Best-effort: if relocation fails (locked file, permissions...), keep running from wherever we
            // are rather than crash the app.
            return false;
        }
#endif
    }
}
