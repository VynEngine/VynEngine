using System.Runtime.InteropServices;
using Photino.NET;

namespace VynEngine.Editor.Helpers;

internal static class WindowsTaskbarHelper
{
    internal static void Initialize() => TaskbarProgress.Init();
    
    internal static void ShowProgress(PhotinoWindow w, bool indeterminate = false)
    {
        TaskbarProgress.SetState(w.WindowHandle, indeterminate ? TBPFLAG.TBPF_INDETERMINATE : TBPFLAG.TBPF_NORMAL);
        TaskbarProgress.SetValue(w.WindowHandle, 0, 100);
    }
    
    internal static void HideProgress(PhotinoWindow w)
    {
        TaskbarProgress.SetState(w.WindowHandle, TBPFLAG.TBPF_NOPROGRESS);
    }
    
    internal static void SetProgressErrored(PhotinoWindow w)
    {
        TaskbarProgress.SetState(w.WindowHandle, TBPFLAG.TBPF_ERROR);
    }
    
    internal static void SetProgressPaused(PhotinoWindow w)
    {
        TaskbarProgress.SetState(w.WindowHandle, TBPFLAG.TBPF_PAUSED);
    }
    
    internal static void UpdateProgress(PhotinoWindow w, ulong current, ulong total)
    {
        TaskbarProgress.SetValue(w.WindowHandle, current, total);
    }
    
    internal enum TBPFLAG
    {
        TBPF_NOPROGRESS    = 0x0,
        TBPF_INDETERMINATE = 0x1, // Marquee
        TBPF_NORMAL        = 0x2, // progress (green)
        TBPF_ERROR         = 0x4, // red
        TBPF_PAUSED        = 0x8  // yellow
    }
    
    private static class TaskbarProgress
    {
        private static readonly ITaskbarList3 _tb = (ITaskbarList3)new CTaskbarList();
        
        public static void Init()
        {
            try { _tb.HrInit(); } catch { /* Explorer evtl. nicht verfügbar */ }
        }

        public static void SetState(IntPtr hwnd, TBPFLAG state) =>
            _tb.SetProgressState(hwnd, state);

        public static void SetValue(IntPtr hwnd, ulong current, ulong total) =>
            _tb.SetProgressValue(hwnd, current, total);
    }
    
    [ComImport]
    [Guid("EA1AFB91-9E28-4B86-90E9-9E9F8A5EEA84")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface ITaskbarList3
    {
        // ITaskbarList
        void HrInit();
        void AddTab(IntPtr hwnd);
        void DeleteTab(IntPtr hwnd);
        void ActivateTab(IntPtr hwnd);
        void SetActiveAlt(IntPtr hwnd);
        // ITaskbarList2
        void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);
        // ITaskbarList3 (Auszug)
        void SetProgressValue(IntPtr hwnd, ulong ullCompleted, ulong ullTotal);
        void SetProgressState(IntPtr hwnd, TBPFLAG tbpFlags);
    }

    [ComImport]
    [Guid("56FDF344-FD6D-11d0-958A-006097C9A090")]
    private class CTaskbarList;
}