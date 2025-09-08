using System.Runtime.InteropServices;
using Photino.NET;

namespace VynEngine.Editor.Helpers;

/// <summary>
/// Windows only helper methods for resizing the main application window.
/// </summary>
internal static class WindowResizeHelper
{
#if WINDOWS
    private const int WM_NCLBUTTONDOWN = 0x00A1;
    private static readonly IntPtr HTLEFT = 10;
    private static readonly IntPtr HTRIGHT = 11;
    private static readonly IntPtr HTTOP = 12;
    private static readonly IntPtr HTTOPLEFT = 13;
    private static readonly IntPtr HTTOPRIGHT = 14;
    private static readonly IntPtr HTBOTTOM = 15;
    private static readonly IntPtr HTBOTTOMLEFT = 16;
    private static readonly IntPtr HTBOTTOMRIGHT = 17;

    [DllImport("user32.dll")] private static extern bool ReleaseCapture();
    [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

    public static void Begin(PhotinoWindow w, string edge, Func<bool> isMaximized, Action restoreBeforeDrag)
    {
        if (isMaximized()) restoreBeforeDrag();

        var ht = edge switch
        {
            "left"        => HTLEFT,
            "right"       => HTRIGHT,
            "top"         => HTTOP,
            "bottom"      => HTBOTTOM,
            "topleft"     => HTTOPLEFT,
            "topright"    => HTTOPRIGHT,
            "bottomleft"  => HTBOTTOMLEFT,
            _ => HTBOTTOMRIGHT
        };

        ReleaseCapture();
        SendMessage(w.WindowHandle, WM_NCLBUTTONDOWN, ht, IntPtr.Zero);
    }
#endif
}