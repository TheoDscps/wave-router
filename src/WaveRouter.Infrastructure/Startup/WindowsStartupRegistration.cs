using Microsoft.Win32;
using WaveRouter.Core.Abstractions;

namespace WaveRouter.Infrastructure.Startup;

/// <summary>Registers WaveRouter to launch at Windows logon via the per-user Run registry key — no admin
/// rights needed, matches the app's single-user, no-installer nature. See docs/use-cases/windows-autostart.md.</summary>
public sealed class WindowsStartupRegistration : IStartupRegistration
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "WaveRouter";

    public bool IsEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
            return key?.GetValue(ValueName) is not null;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool SetEnabled(bool enabled)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
            if (key is null)
            {
                return false;
            }

            if (enabled)
            {
                var exePath = Environment.ProcessPath;
                if (string.IsNullOrEmpty(exePath))
                {
                    return false;
                }

                key.SetValue(ValueName, $"\"{exePath}\"");
            }
            else
            {
                key.DeleteValue(ValueName, throwOnMissingValue: false);
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>Points the Run key at an explicit exe path rather than the current process's own path — used
    /// by <see cref="SelfRelocation"/>, where the process making the call is the OLD exe about to be replaced,
    /// not the relocated one the key should end up pointing at.</summary>
    internal bool SetEnabledForExePath(string exePath)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
            if (key is null)
            {
                return false;
            }

            key.SetValue(ValueName, $"\"{exePath}\"");
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
