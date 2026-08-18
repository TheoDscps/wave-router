using System.Runtime.InteropServices;
using NAudio.CoreAudioApi;

namespace WaveRouter.Infrastructure.Audio.PolicyConfig;

/// <summary>
/// Same as <see cref="IAudioPolicyConfigFactoryVariantFor21H2"/> but for Windows builds &lt; 21390 —
/// the vtable shape is identical, only the interface GUID differs between OS versions. Ported from
/// EarTrumpet (https://github.com/File-New-Project/EarTrumpet, MIT). See that file's XML doc for why
/// this is declared InterfaceIsIUnknown with 3 explicit IInspectable padding methods instead of the
/// technically-correct InterfaceIsIInspectable.
/// </summary>
[Guid("2a59116d-6c4f-45e0-a74f-707e3fef9258")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IAudioPolicyConfigFactoryVariantForDownlevel
{
    int __incomplete__GetIids();
    int __incomplete__GetRuntimeClassName();
    int __incomplete__GetTrustLevel();

    int __incomplete__add_CtxVolumeChange();
    int __incomplete__remove_CtxVolumeChanged();
    int __incomplete__add_RingerVibrateStateChanged();
    int __incomplete__remove_RingerVibrateStateChange();
    int __incomplete__SetVolumeGroupGainForId();
    int __incomplete__GetVolumeGroupGainForId();
    int __incomplete__GetActiveVolumeGroupForEndpointId();
    int __incomplete__GetVolumeGroupsForEndpoint();
    int __incomplete__GetCurrentVolumeContext();
    int __incomplete__SetVolumeGroupMuteForId();
    int __incomplete__GetVolumeGroupMuteForId();
    int __incomplete__SetRingerVibrateState();
    int __incomplete__GetRingerVibrateState();
    int __incomplete__SetPreferredChatApplication();
    int __incomplete__ResetPreferredChatApplication();
    int __incomplete__GetPreferredChatApplication();
    int __incomplete__GetCurrentChatApplications();
    int __incomplete__add_ChatContextChanged();
    int __incomplete__remove_ChatContextChanged();

    [PreserveSig]
    HRESULT SetPersistedDefaultAudioEndpoint(uint processId, DataFlow flow, Role role, IntPtr deviceId);

    /// <summary>deviceId is a raw HSTRING handle (see <see cref="Combase.ReadAndDeleteHString"/>) —
    /// [MarshalAs(UnmanagedType.HString)] hits the same "not supported in the .NET runtime" wall as
    /// IInspectable does elsewhere in this port.</summary>
    [PreserveSig]
    HRESULT GetPersistedDefaultAudioEndpoint(uint processId, DataFlow flow, Role role, out IntPtr deviceId);

    [PreserveSig]
    HRESULT ClearAllPersistedApplicationDefaultEndpoints();
}
