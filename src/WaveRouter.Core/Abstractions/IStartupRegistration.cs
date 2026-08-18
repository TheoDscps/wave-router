namespace WaveRouter.Core.Abstractions;

/// <summary>Registers/unregisters the app to launch at Windows logon. See
/// docs/use-cases/windows-autostart.md.</summary>
public interface IStartupRegistration
{
    bool IsEnabled();

    /// <returns>False if registration/unregistration failed (e.g. permission issue) — the caller should
    /// revert any optimistic UI state in that case.</returns>
    bool SetEnabled(bool enabled);
}
