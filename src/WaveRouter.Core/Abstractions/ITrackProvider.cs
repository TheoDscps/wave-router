namespace WaveRouter.Core.Abstractions;

/// <summary>Lists the tracks a rule can target. See docs/use-cases/read-wave-link-tracks.md.</summary>
public interface ITrackProvider
{
    /// <summary>Empty when no matching output device is found (e.g. Wave Link not installed) — callers
    /// should fall back to letting the user type a track name manually.</summary>
    IReadOnlyList<string> GetAvailableTracks();
}
