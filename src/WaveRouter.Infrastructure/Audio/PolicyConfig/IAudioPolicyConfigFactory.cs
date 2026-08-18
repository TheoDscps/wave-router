using NAudio.CoreAudioApi;

namespace WaveRouter.Infrastructure.Audio.PolicyConfig;

/// <summary>OS-version-agnostic view over the two real WinRT interface shapes (see the
/// VariantFor21H2 / VariantForDownlevel implementations). NAudio's <see cref="DataFlow"/> and
/// <see cref="Role"/> enums share the exact underlying values as the native EDataFlow/ERole,
/// so they're reused here instead of redeclaring them.</summary>
internal interface IAudioPolicyConfigFactory
{
    HRESULT SetPersistedDefaultAudioEndpoint(uint processId, DataFlow flow, Role role, IntPtr deviceId);
}
