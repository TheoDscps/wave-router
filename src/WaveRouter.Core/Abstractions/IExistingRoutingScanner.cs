namespace WaveRouter.Core.Abstractions;

/// <summary>Reads per-app audio routing already configured at the Windows level (Settings → System →
/// Sound → "App volume and device preferences") — e.g. assignments the user made manually before ever
/// using WaveRouter. Wave Link itself doesn't store this; Windows does.</summary>
public interface IExistingRoutingScanner
{
    /// <summary>The Wave Link track <paramref name="processId"/> is already assigned to, or null if
    /// there's no assignment, or if it points to a non-Wave-Link device.</summary>
    string? GetExistingTrackAssignment(int processId);
}
