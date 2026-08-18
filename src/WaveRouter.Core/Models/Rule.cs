namespace WaveRouter.Core.Models;

/// <summary>A routing rule: when <see cref="ExecutableName"/> produces audio, route it to <see cref="TrackName"/>.</summary>
public sealed record Rule(string ExecutableName, string TrackName);
