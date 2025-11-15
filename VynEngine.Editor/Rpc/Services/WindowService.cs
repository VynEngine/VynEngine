using VynEngine.Editor.UI;
using VynEngine.Editor.UI.Helpers;

namespace VynEngine.Editor.Rpc.Services;

/// <summary>
/// The window service provides methods to interact with the main application window, such as resizing, moving, and changing the title.
/// </summary>
[RpcService("window")]
internal sealed class WindowService
{
    /// <summary>
    /// The header UI component associated with the main application window.
    /// </summary>
    public static Header Header { get; } = new();
    
    /// <summary>
    /// Returns true if the application is running on Windows, false otherwise.
    /// </summary>
    [RpcMethod]
    public bool IsWindows()
    {
#if WINDOWS
        return true;
#else
        return false;
#endif
    }
    
    /// <summary>
    /// Sets the title of the main application window.
    /// </summary>
    /// <param name="title">The new title for the window.</param>
    [RpcMethod]
    public void SetTitle(string title) => Program.Window.SetTitle(title);

    /// <summary>
    /// Maximizes or restores the main application window.
    /// </summary>
    /// <param name="maximized">True to maximize the window, false to restore it.</param>
    [RpcMethod]
    public void SetMaximized(bool maximized) => Program.CustomMaximize(maximized);

    /// <summary>
    /// Minimizes or restores the main application window.
    /// </summary>
    /// <param name="minimized">True to minimize the window, false to restore it.</param>
    [RpcMethod]
    public void SetMinimized(bool minimized) => Program.Window.SetMinimized(minimized);

    /// <summary>
    /// Starts the drag operation for moving the main application window.
    /// </summary>
    [RpcMethod]
    public void BeginDrag() => Program.BeginDrag();
    
    /// <summary>
    /// Starts the resize operation for resizing the main application window in the specified direction.
    /// </summary>
    /// <param name="direction">The direction of the resize operation.</param>
    [RpcMethod]
    public void BeginResize(string direction) => Program.BeginResize(direction);
    
    /// <summary>
    /// Closes the main application window.
    /// </summary>
    [RpcMethod]
    public void Close() => Program.Window.Close();

    /// <summary>
    /// Shows the taskbar progress indicator on Windows. If 'indeterminate' is true, the progress will be shown as a marquee.
    /// </summary>
    [RpcMethod]
    public void ShowTaskbarProgress(bool indeterminate)
    {
#if WINDOWS
        WindowsTaskbarHelper.ShowProgress(Program.Window, indeterminate);
#endif
    }
    
    /// <summary>
    /// Hides the taskbar progress indicator on Windows.
    /// </summary>
    [RpcMethod]
    public void HideTaskbarProgress()
    {
#if WINDOWS
        WindowsTaskbarHelper.HideProgress(Program.Window);
#endif
    }
    
    /// <summary>
    /// Sets the taskbar progress indicator on Windows to an errored state, typically shown as red.
    /// </summary>
    [RpcMethod]
    public void SetTaskbarProgressErrored()
    {
#if WINDOWS
        WindowsTaskbarHelper.SetProgressErrored(Program.Window);
#endif
    }
    
    /// <summary>
    /// Sets the taskbar progress indicator on Windows to a paused state, typically shown as yellow.
    /// </summary>
    [RpcMethod]
    public void SetTaskbarProgressPaused()
    {
#if WINDOWS
        WindowsTaskbarHelper.SetProgressPaused(Program.Window);
#endif
    }
    
    /// <summary>
    /// Updates the taskbar progress indicator on Windows to reflect the current progress.
    /// </summary>
    [RpcMethod]
    public void UpdateTaskbarProgress(double percent)
    {
#if WINDOWS
        var current = (ulong)(percent * 100);
        WindowsTaskbarHelper.UpdateProgress(Program.Window, current, 100);
#endif
    }
    
    /// <summary>
    /// Invokes the click event for the header menu item with the specified ID.
    /// </summary>
    [RpcMethod]
    public void InvokeHeaderMenuClicked(string id)
    {
        Header.FileMenu.InvokeClicked(id);
        Header.EditMenu.InvokeClicked(id);
        Header.ViewMenu.InvokeClicked(id);
        Header.HelpMenu.InvokeClicked(id);
    }
}