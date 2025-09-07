namespace VynEngine.Editor.Rpc.Services;

/// <summary>
/// The window service provides methods to interact with the main application window, such as resizing, moving, and changing the title.
/// </summary>
[RpcService("window")]
internal sealed class WindowService
{
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
    /// Closes the main application window.
    /// </summary>
    [RpcMethod]
    public void Close() => Program.Window.Close();
    
    /// <summary>
    /// Changes the subtitle of the main application window. This is typically displayed below the main title.
    /// </summary>
    /// <param name="subtitle">The new subtitle for the window. If empty, the subtitle will be reset.</param>
    public static void ChangeSubtitle(string subtitle = "") => Program.Emit("window", "updateSubtitle", subtitle);
}