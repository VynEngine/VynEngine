using System.Drawing;
using Photino.NET;
using Photino.NET.Server;
using Serilog;
using Serilog.Core;
using VynEngine.Editor.Rpc;
using VynEngine.Editor.Rpc.Services;
using VynEngine.Editor.UI.Helpers;

namespace VynEngine.Editor;

/// <summary>
/// The main entry point of the editor. The editor runs on Photino.NET and Vue 3. It adds a thin C# bridge layer to
/// allow direct interaction in C# to specific parts of the Vue 3 application.
/// </summary>
internal class Program
{
    /// <summary>
    /// The logger instance for logging application events and errors.
    /// </summary>
    internal static Logger Log { get; } = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.Console()
        .WriteTo.File("latest.log")
        .CreateLogger();

    /// <summary>
    /// The main Photino window instance for the editor application.
    /// </summary>
    internal static PhotinoWindow Window { get; private set; } = null!;
    
    private const string DefaultWindowTitle = "VynEngine";
    private static bool _isMaximized;
    
    [STAThread]
    private static void Main(string[] args)
    {
        _ = WindowService.Header; // make sure the header is initialized
        
        RpcServer.RegisterServicesFromAssembly(typeof(Program).Assembly);

#if WINDOWS
        WindowsTaskbarHelper.Initialize();
#endif
        
        PhotinoServer
            .CreateStaticFileServer(args, out var baseUrl)
            .RunAsync();

#if DEBUG
        const string appUrl = "http://localhost:5173";
#else
        const string appUrl = $"{baseUrl}/index.html";
#endif
        Log.Information("Loading application from {AppUrl}", appUrl);

        Window = new PhotinoWindow()
            .SetTitle(DefaultWindowTitle)
            .SetUseOsDefaultSize(false)
            .SetResizable(true)
#if WINDOWS
            .SetChromeless(true)
#endif
            .SetSize(new Size(1280, 720))
            .Load(appUrl);
        
        RpcServer.Attach(Window);
        
        Window.ContextMenuEnabled = false;

#if DEBUG
        Window.DevToolsEnabled = true;
#else
        Window.WindowCreated += (_, _) =>
        {
            CustomMaximize(true);
        };
#endif

        Window.WaitForClose();
    }

    /// <summary>
    /// Custom maximize implementation to handle maximization without using OS-level maximization.
    /// This allows for a more controlled behavior, especially in chromeless windows.
    /// </summary>
    /// <param name="maximize">True to maximize the window, false to restore it.</param>
    internal static void CustomMaximize(bool maximize)
    {
        if (maximize == _isMaximized) return;

        if (maximize)
        {
#if WINDOWS
            WindowResizeHelper.EnterCustomMaximize(Window);
#endif
        }
        else
        {
#if WINDOWS
            WindowResizeHelper.ExitCustomMaximize(Window);
#endif
        }

        _isMaximized = maximize;
        Emit("window", "updateMaximized", maximize);
    }

    /// <summary>
    /// Begins the drag operation for moving the main application window. This is typically called when the user clicks
    /// and drags the title bar area. This is currently only supported on Windows.
    /// </summary>
    internal static void BeginDrag()
    {
#if WINDOWS
        WindowResizeHelper.GetSavedState(Window, out var size, out var pos);
        WindowDragHelper.BeginDragFromTitlebar(Window, ref _isMaximized, size, ref pos);
#endif
    }

    /// <summary>
    /// Begins the resize operation for resizing the main application window. This is typically called when the user clicks
    /// and drags the window borders or corners. This is currently only supported on Windows.
    /// </summary>
    /// <param name="direction">The direction of the resize operation.</param>
    internal static void BeginResize(string direction)
    {
#if WINDOWS
        WindowResizeHelper.Begin(Window, direction, () => _isMaximized, () =>
        {
            CustomMaximize(false);
            Emit("window", "updateMaximized", false);
        });
#endif
    }
    
    /// <summary>
    /// Emits a custom event to the JavaScript side. This is a one-way notification and does not expect a response.
    /// </summary>
    /// <param name="svc">The service name.</param>
    /// <param name="name">The event name.</param>
    /// <param name="data">The event data (optional).</param>
    internal static void Emit(string svc, string name, object? data = null)
    {
        RpcServer.Emit(Window, svc, name, data);
    }

    /// <summary>
    /// Emits an SVG event to the JavaScript side. This is a one-way notification and does not expect a response.
    /// </summary>
    /// <param name="svc">The service name.</param>
    /// <param name="name">The event name.</param>
    /// <param name="data">The event data as variadic parameters.</param>
    internal static void Emit(string svc, string name, params object[] data)
    {
        RpcServer.Emit(Window, svc, name, data);
    }
}