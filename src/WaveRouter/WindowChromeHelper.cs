using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace WaveRouter;

/// <summary>
/// WPF windows keep the OS's default light title bar/border regardless of the app's own dark theme —
/// this forces the native chrome into dark mode (same DWM attribute apps like VS Code and Windows
/// Terminal use), without giving up native drag/resize/snap behavior via a fully custom chrome.
/// </summary>
internal static class WindowChromeHelper
{
    private const int DwmwaUseImmersiveDarkMode = 20; // Windows 10 20H1+ / Windows 11

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

    public static void ApplyDarkTitleBar(Window window)
    {
        window.SourceInitialized += (_, _) =>
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            var enabled = 1;
            DwmSetWindowAttribute(hwnd, DwmwaUseImmersiveDarkMode, ref enabled, sizeof(int));
        };
    }
}
