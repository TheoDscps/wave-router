using System.Runtime.InteropServices;

namespace WaveRouter.Infrastructure.Audio.PolicyConfig;

/// <summary>P/Invoke onto combase.dll, needed to activate the undocumented WinRT-internal
/// "Windows.Media.Internal.AudioPolicyConfig" runtime class (see <see cref="AudioPolicyConfigFactorySelector"/>).
/// Two adjustments versus the .NET Framework reference implementation this is ported from (EarTrumpet),
/// both because modern .NET dropped the automatic WinRT interop it relied on:
/// activatable class IDs are passed as raw HSTRING handles (built via <see cref="WindowsCreateString"/>)
/// instead of <c>[MarshalAs(UnmanagedType.HString)]</c> on the signature ("Invalid managed/unmanaged type
/// combination" otherwise); and the returned factory is a raw <see cref="IntPtr"/> instead of
/// <c>[MarshalAs(UnmanagedType.IInspectable)] out object</c> ("Marshalling as IInspectable is not supported
/// in the .NET runtime" otherwise) — callers convert it with <see cref="Marshal.GetTypedObjectForIUnknown"/>.</summary>
internal static class Combase
{
    [DllImport("combase.dll", PreserveSig = false)]
    public static extern void RoGetActivationFactory(
        IntPtr activatableClassId,
        [In] ref Guid iid,
        out IntPtr factory);

    [DllImport("combase.dll", PreserveSig = false)]
    public static extern void WindowsCreateString(
        [MarshalAs(UnmanagedType.LPWStr)] string sourceString,
        [In] uint length,
        [Out] out IntPtr hstring);

    [DllImport("combase.dll", PreserveSig = false)]
    public static extern void WindowsDeleteString(IntPtr hstring);

    /// <summary>Native signature returns the buffer pointer directly (not an HRESULT) — no PreserveSig needed.</summary>
    [DllImport("combase.dll")]
    private static extern IntPtr WindowsGetStringRawBuffer(IntPtr hstring, out uint length);

    /// <summary>Creates an HSTRING for <paramref name="value"/>, runs <paramref name="action"/> with it, and
    /// always deletes it afterward — HSTRINGs are not GC-tracked and must be released explicitly.</summary>
    public static void WithHString(string value, Action<IntPtr> action)
    {
        WindowsCreateString(value, (uint)value.Length, out var hstring);
        try
        {
            action(hstring);
        }
        finally
        {
            WindowsDeleteString(hstring);
        }
    }

    /// <summary>Reads an HSTRING returned from an out-parameter (e.g. GetPersistedDefaultAudioEndpoint) into
    /// a managed string, then releases it. HSTRING.IntPtr.Zero (the empty string singleton) reads as "".</summary>
    public static string ReadAndDeleteHString(IntPtr hstring)
    {
        if (hstring == IntPtr.Zero)
        {
            return string.Empty;
        }

        try
        {
            var buffer = WindowsGetStringRawBuffer(hstring, out var length);
            return buffer == IntPtr.Zero ? string.Empty : Marshal.PtrToStringUni(buffer, (int)length);
        }
        finally
        {
            WindowsDeleteString(hstring);
        }
    }
}
