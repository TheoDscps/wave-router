namespace WaveRouter.Infrastructure.Audio.PolicyConfig;

internal static class AudioPolicyConfigFactorySelector
{
    /// <summary>Windows 10 21H2 (and Windows 11, which is newer) exposes a different vtable shape for
    /// the same runtime class than earlier builds — this is the build number where it changed.</summary>
    private const int Build21H2 = 21390;

    public static IAudioPolicyConfigFactory Create() =>
        Environment.OSVersion.Version.Build >= Build21H2
            ? new AudioPolicyConfigFactoryImplFor21H2()
            : new AudioPolicyConfigFactoryImplForDownlevel();
}
