using WaveRouter.Core.Models;

namespace WaveRouter.Core.Abstractions;

/// <summary>Reads Wave Link's own Automixer state — which apps are assigned to which track — straight
/// from its config file, rather than Windows' per-app default-device setting. Unlike
/// <see cref="IExistingRoutingScanner"/>, this doesn't require the app to be currently running: Wave
/// Link remembers apps it has seen before.</summary>
public interface IWaveLinkMixerConfigReader
{
    /// <summary>Empty if the config can't be found or parsed (undocumented, private format — a future
    /// Wave Link update could change it at any time).</summary>
    IReadOnlyList<KnownAppAssignment> ReadKnownAssignments();
}
