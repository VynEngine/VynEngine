using System.Drawing;
using System.Runtime.InteropServices;
using Photino.NET;
using VynEngine.Editor.Rpc;

namespace VynEngine.Editor.Helpers;

/// <summary>
/// Windows only drag helper to allow dragging the window from a custom title bar.
/// </summary>
internal static class WindowDragHelper
{
#if WINDOWS
    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

    [DllImport("user32.dll")]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

    private const int WM_NCLBUTTONDOWN = 0x00A1;
    private static readonly IntPtr HTCAPTION = 2;

    private const uint MONITOR_DEFAULTTONEAREST = 2;

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int left, top, right, bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    public static void BeginDragFromTitlebar(PhotinoWindow window, ref bool isMaximized, Size normalSize, ref Point normalPos)
    {
        GetCursorPos(out var cursor);

        if (isMaximized)
        {
            Program.Emit("window", "updateMaximized", false);
            
            var hmon = MonitorFromPoint(cursor, MONITOR_DEFAULTTONEAREST);
            var mi = new MONITORINFO { cbSize = Marshal.SizeOf<MONITORINFO>() };
            GetMonitorInfo(hmon, ref mi);
            var work = mi.rcWork;

            var anchor = (cursor.X - work.left) / (double)Math.Max(1, (work.right - work.left));
            anchor = Math.Clamp(anchor, 0.1, 0.9);

            Program.CustomMaximize(false);
            isMaximized = false;

            var targetLeft = cursor.X - (int)(normalSize.Width * anchor);
            var minLeft = work.left;
            var maxLeft = work.right - normalSize.Width;
            targetLeft = Math.Clamp(targetLeft, minLeft, maxLeft);

            var targetTop = work.top;

            window.Location = new Point(targetLeft, targetTop);
            window.SetSize(normalSize.Width, normalSize.Height);

            normalPos = new Point(targetLeft, targetTop);
        }

        ReleaseCapture();
        SendMessage(window.WindowHandle, WM_NCLBUTTONDOWN, HTCAPTION, IntPtr.Zero);
    }
#endif
}