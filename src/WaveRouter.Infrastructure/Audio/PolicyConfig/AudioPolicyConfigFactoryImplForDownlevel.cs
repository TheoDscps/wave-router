using System.Runtime.InteropServices;
using NAudio.CoreAudioApi;

namespace WaveRouter.Infrastructure.Audio.PolicyConfig;

internal sealed class AudioPolicyConfigFactoryImplForDownlevel : IAudioPolicyConfigFactory
{
    private readonly IAudioPolicyConfigFactoryVariantForDownlevel _factory;

    public AudioPolicyConfigFactoryImplForDownlevel()
    {
        var factoryPtr = IntPtr.Zero;
        Combase.WithHString("Windows.Media.Internal.AudioPolicyConfig", classId =>
        {
            var iid = typeof(IAudioPolicyConfigFactoryVariantForDownlevel).GUID;
            Combase.RoGetActivationFactory(classId, ref iid, out factoryPtr);
        });

        _factory = (IAudioPolicyConfigFactoryVariantForDownlevel)Marshal.GetTypedObjectForIUnknown(
            factoryPtr, typeof(IAudioPolicyConfigFactoryVariantForDownlevel));
        Marshal.Release(factoryPtr);
    }

    public HRESULT SetPersistedDefaultAudioEndpoint(uint processId, DataFlow flow, Role role, IntPtr deviceId) =>
        _factory.SetPersistedDefaultAudioEndpoint(processId, flow, role, deviceId);

    public HRESULT GetPersistedDefaultAudioEndpoint(uint processId, DataFlow flow, Role role, out IntPtr deviceId) =>
        _factory.GetPersistedDefaultAudioEndpoint(processId, flow, role, out deviceId);
}
