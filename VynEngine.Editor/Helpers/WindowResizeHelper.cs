using System.Collections.Concurrent;
using System.Drawing;
using System.Runtime.InteropServices;
using Photino.NET;

namespace VynEngine.Editor.Helpers;

/// <summary>
/// Windows only helper methods for resizing the main application window.
/// </summary>
internal static class WindowResizeHelper
{
#if WINDOWS

    #region Win32 API Definitions

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
    
    [DllImport("user32.dll", SetLastError = true)] private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
    [DllImport("user32.dll", SetLastError = true)] private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    [DllImport("user32.dll")] private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);
    [DllImport("user32.dll", SetLastError = true)] private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);
    [DllImport("user32.dll")] private static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);
    [DllImport("user32.dll")] private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);
    [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    
    private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const int SW_RESTORE = 9;

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left, Top, Right, Bottom;
        public int Width => Right - Left;
        public int Height => Bottom - Top;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor; // full monitor
        public RECT rcWork;    // work area (excludes taskbar)
        public uint dwFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WINDOWPLACEMENT
    {
        public int length;
        public int flags;
        public int showCmd;
        public POINT ptMinPosition;
        public POINT ptMaxPosition;
        public RECT rcNormalPosition;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT { public int X; public int Y; }

    #endregion
    
    private sealed class WindowRestoreState
    {
        public Size Size { get; set; }
        
        public Point Position { get; set; }
        
        public IntPtr Monitor { get; set; }
    }
    
    private static readonly ConcurrentDictionary<IntPtr, WindowRestoreState> SavedStates = new();

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
    
    public static void EnterCustomMaximize(PhotinoWindow w)
    {
        var hWnd = w.WindowHandle;
        var size = w.Size;
        var pos = w.Location;

        var hMon = MonitorFromWindow(hWnd, MONITOR_DEFAULTTONEAREST);
        var state = new WindowRestoreState { Size = size, Position = pos, Monitor = hMon };
        SavedStates[hWnd] = state;

        var mi = new MONITORINFO { cbSize = Marshal.SizeOf<MONITORINFO>() };
        if (!GetMonitorInfo(hMon, ref mi))
            return;

        var wa = mi.rcWork;
        SetWindowPos(hWnd, IntPtr.Zero, wa.Left, wa.Top, wa.Width, wa.Height, SWP_NOZORDER | SWP_NOACTIVATE);
    }
    
    public static void GetSavedState(PhotinoWindow w, out Size size, out Point pos)
    {
        var hWnd = w.WindowHandle;
        if (!SavedStates.TryGetValue(hWnd, out var state))
        {
            size = Size.Empty;
            pos = Point.Empty;
            return;
        }

        size = state.Size;
        pos = state.Position;
    }
    
    public static void ExitCustomMaximize(PhotinoWindow w)
    {
        var hWnd = w.WindowHandle;
        if (!SavedStates.TryRemove(hWnd, out var state))
            return;
        
        w.Size = state.Size;
        w.Location = state.Position;
    }
#endif
}