namespace WaveRouter.Core.Models;

/// <summary>One routing decision, for display in the history view. Session-only — never persisted to disk.</summary>
public sealed record RoutingHistoryEntry(
    DateTime Timestamp,
    string DisplayName,
    string ProcessName,
    string TrackName,
    bool Success,
    string? ErrorMessage);
