using System.Runtime.InteropServices;
using NAudio.CoreAudioApi;

namespace WaveRouter.Infrastructure.Audio.PolicyConfig;

/// <summary>
/// Undocumented WinRT interface for Windows builds &gt;= 21390 (21H2+). Ported from EarTrumpet
/// (https://github.com/File-New-Project/EarTrumpet, MIT), which reverse-engineered this shape —
/// there is no public Microsoft documentation for it. The leading "__incomplete__" members are
/// deliberate vtable padding for methods this app doesn't call; only their slot position matters.
///
/// Declared as <see cref="ComInterfaceType.InterfaceIsIUnknown"/> rather than the technically-correct
/// InterfaceIsIInspectable: modern .NET dropped IInspectable marshaling support entirely ("Marshalling
/// as IInspectable is not supported in the .NET runtime"), even for a raw pointer wrapped via
/// Marshal.GetTypedObjectForIUnknown. IInspectable's vtable is ABI-compatible with IUnknown plus 3 extra
/// methods (GetIids, GetRuntimeClassName, GetTrustLevel) — declaring those 3 explicitly as padding
/// below keeps every subsequent slot, including the real methods, at the correct offset.
/// </summary>
[Guid("ab3d4648-e242-459f-b02f-541c70306324")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IAudioPolicyConfigFactoryVariantFor21H2
{
    // IInspectable's own 3 methods (this interface extends IInspectable, not IUnknown directly).
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

    [PreserveSig]
    HRESULT GetPersistedDefaultAudioEndpoint(uint processId, DataFlow flow, Role role, [Out, MarshalAs(UnmanagedType.HString)] out string deviceId);

    [PreserveSig]
    HRESULT ClearAllPersistedApplicationDefaultEndpoints();
}
