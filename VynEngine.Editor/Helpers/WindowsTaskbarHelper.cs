using System.Runtime.InteropServices;
using Photino.NET;

namespace VynEngine.Editor.Helpers;

internal static class WindowsTaskbarHelper
{
    internal static void Initialize() => TaskbarProgress.Initialize();
    
    internal static void ShowProgress(PhotinoWindow w, bool indeterminate = false)
    {
        TaskbarProgress.SetState(w.WindowHandle, indeterminate ? TaskbarProgress.TaskbarStates.Indeterminate : TaskbarProgress.TaskbarStates.Normal);
    }
    
    internal static void HideProgress(PhotinoWindow w)
    {
        TaskbarProgress.SetState(w.WindowHandle, TaskbarProgress.TaskbarStates.NoProgress);
    }
    
    internal static void SetProgressErrored(PhotinoWindow w)
    {
        TaskbarProgress.SetState(w.WindowHandle, TaskbarProgress.TaskbarStates.Error);
    }
    
    internal static void SetProgressPaused(PhotinoWindow w)
    {
        TaskbarProgress.SetState(w.WindowHandle, TaskbarProgress.TaskbarStates.Paused);
    }
    
    internal static void UpdateProgress(PhotinoWindow w, ulong current, ulong total)
    {
        TaskbarProgress.SetValue(w.WindowHandle, current, total);
    }
    
    private static class TaskbarProgress
    {
        public enum TaskbarStates
        {
            NoProgress    = 0,
            Indeterminate = 0x1,
            Normal        = 0x2,
            Error         = 0x4,
            Paused        = 0x8
        }

        [ComImport]
        [Guid("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ITaskbarList3
        {
            // ITaskbarList
            [PreserveSig]
            void HrInit();
            [PreserveSig]
            void AddTab(IntPtr hwnd);
            [PreserveSig]
            void DeleteTab(IntPtr hwnd);
            [PreserveSig]
            void ActivateTab(IntPtr hwnd);
            [PreserveSig]
            void SetActiveAlt(IntPtr hwnd);

            // ITaskbarList2
            [PreserveSig]
            void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);

            // ITaskbarList3
            [PreserveSig]
            void SetProgressValue(IntPtr hwnd, UInt64 ullCompleted, UInt64 ullTotal);
            [PreserveSig]
            void SetProgressState(IntPtr hwnd, TaskbarStates state);
        }

        [ComImport]    
        [Guid("56fdf344-fd6d-11d0-958a-006097c9a090")]
        [ClassInterface(ClassInterfaceType.None)]
        private class TaskbarInstance;

        private static ITaskbarList3 taskbarInstance = (ITaskbarList3)new TaskbarInstance();
        private static bool taskbarSupported = Environment.OSVersion.Version >= new Version(6, 1);

        public static void SetState(IntPtr windowHandle, TaskbarStates taskbarState)
        {
            if (taskbarSupported) taskbarInstance.SetProgressState(windowHandle, taskbarState);
        }

        public static void SetValue(IntPtr windowHandle, double progressValue, double progressMax)
        {
            if (taskbarSupported) taskbarInstance.SetProgressValue(windowHandle, (ulong)progressValue, (ulong)progressMax);
        }
        
        public static void Initialize()
        {
            if (taskbarSupported) taskbarInstance.HrInit();
        }
    }
}