namespace WaveRouter.Core.Models;

/// <summary>An app-to-track association Wave Link's own Automixer already knows about, whether or not
/// the app is currently running. See docs/use-cases/read-wave-link-tracks.md.</summary>
public sealed record KnownAppAssignment(string ExecutableName, string TrackName);
