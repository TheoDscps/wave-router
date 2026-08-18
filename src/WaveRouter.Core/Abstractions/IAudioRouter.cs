using WaveRouter.Core.Models;

namespace WaveRouter.Core.Abstractions;

/// <summary>Applies a rule's routing: switches a process's default audio output device.
/// See docs/use-cases/automatic-routing-enforcement.md. Implementations must never throw —
/// this wraps an undocumented Windows API and failures are expected (missing device, unsupported OS, etc).</summary>
public interface IAudioRouter
{
    /// <summary>Routes <paramref name="processId"/>'s render audio to the output device whose friendly
    /// name matches <paramref name="trackName"/> (case-insensitive, substring match).</summary>
    RoutingResult ApplyRule(int processId, string trackName);
}
