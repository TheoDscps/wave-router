using System.Runtime.InteropServices;
using NAudio.CoreAudioApi;

namespace WaveRouter.Infrastructure.Audio.PolicyConfig;

internal sealed class AudioPolicyConfigFactoryImplFor21H2 : IAudioPolicyConfigFactory
{
    private readonly IAudioPolicyConfigFactoryVariantFor21H2 _factory;

    public AudioPolicyConfigFactoryImplFor21H2()
    {
        var factoryPtr = IntPtr.Zero;
        Combase.WithHString("Windows.Media.Internal.AudioPolicyConfig", classId =>
        {
            var iid = typeof(IAudioPolicyConfigFactoryVariantFor21H2).GUID;
            Combase.RoGetActivationFactory(classId, ref iid, out factoryPtr);
        });

        _factory = (IAudioPolicyConfigFactoryVariantFor21H2)Marshal.GetTypedObjectForIUnknown(
            factoryPtr, typeof(IAudioPolicyConfigFactoryVariantFor21H2));
        Marshal.Release(factoryPtr);
    }

    public HRESULT SetPersistedDefaultAudioEndpoint(uint processId, DataFlow flow, Role role, IntPtr deviceId) =>
        _factory.SetPersistedDefaultAudioEndpoint(processId, flow, role, deviceId);

    public HRESULT GetPersistedDefaultAudioEndpoint(uint processId, DataFlow flow, Role role, out IntPtr deviceId) =>
        _factory.GetPersistedDefaultAudioEndpoint(processId, flow, role, out deviceId);
}
