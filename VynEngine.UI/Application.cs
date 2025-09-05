using System.Collections.Concurrent;
using System.Numerics;
using ImGuiNET;
using Silk.NET.Core;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using VynEngine.Core.Exceptions;

namespace VynEngine.UI;

/// <summary>
/// The application is the main entry point for the UI system. It manages the main window, the ImGui context,
/// and the rendering loop. It is responsible for initializing and shutting down the ImGui context,
/// as well as handling input and rendering. It also manages multiple UI windows and their layers.
/// </summary>
public sealed class Application : IDisposable
{
    /// <summary>
    /// Singleton instance of the application. Only one instance is allowed.
    /// </summary>
    internal static Application Instance { get; private set; } = null!;
    
    /// <summary>
    /// The ID of the main thread that created the application. This can be used to ensure that certain operations
    /// are only performed on the main thread, as required by ImGui and many UI frameworks.
    /// </summary>
    internal int MainThreadId { get; set; } = Environment.CurrentManagedThreadId;
    
    /// <summary>
    /// The title bar chrome settings for the application window. This is currently mostly unimplemented. In the future, maybe ;)
    /// </summary>
    public TitleBarChrome Chrome { get; }
    
    /// <summary>
    /// The notification service for displaying toast notifications to the user.
    /// </summary>
    public NotificationService Notifications { get; } = new();

    /// <summary>
    /// The icon of the application window.
    /// </summary>
    public RawImage? Icon
    {
        get => _icon;
        set
        {
            _icon = value;
            
            if (_started)
            {
                UpdateIcon();
            }
        }
    }
    
    /// <summary>
    /// The native window created and managed by the application.
    /// </summary>
    public IWindow NativeWindow { get; }

    private readonly List<Window> _uiWindows = [];
    private readonly ConcurrentQueue<Action> _uiActions = new();
    private GL? _gl;
    private IInputContext? _input;
    private ImGuiGlRenderer? _renderer;
    private RawImage? _icon;
    private IntPtr _imguiCtx;
    private bool _started, _disposed;

    public Application(string title = "VynEngine", Vector2? initialSize = null)
    {
        if (Instance != null) throw new UIException("Only one instance of Application is allowed.");
        Instance = this;
        
        initialSize ??= new Vector2(1280, 720);
        var maxmize = initialSize.Value.X <= 0 || initialSize.Value.Y <= 0;
        if (maxmize) initialSize = new Vector2(1280, 720);
        
        Chrome = new TitleBarChrome
        {
            Title = title
        };
        
        var opts = WindowOptions.Default with
        {
            Title = Chrome.Title,
            Size = new Vector2D<int>((int)initialSize.Value.X, (int)initialSize.Value.Y),
            API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.ForwardCompatible,
                new APIVersion(3, 3)),
            VSync = true,
            IsEventDriven = false,
        };

        NativeWindow = Silk.NET.Windowing.Window.Create(opts);
        NativeWindow.Load += OnLoad;
        NativeWindow.Render += OnRender;
        NativeWindow.Resize += OnResize;
        NativeWindow.Closing += OnClosing;

        if (maxmize) NativeWindow.WindowState = WindowState.Maximized;
    }

    /// <summary>
    /// Adds a new UI window to the application. The window will be managed and rendered by the application.
    /// If no windows are added before starting the application, an exception will be thrown.
    /// </summary>
    /// <param name="uiWindow">The UI window to add.</param>
    public void AddWindow(Window uiWindow) => _uiWindows.Add(uiWindow);

    /// <summary>
    /// Starts the application's main loop. This will block until the application is closed. If the application
    /// has already been started, this method will return immediately. If no UI windows have been added, an exception
    /// will be thrown.
    /// </summary>
    /// <exception cref="UIException">No UI windows added to the application.</exception>
    public void Start()
    {
        if (_started) return;
        _started = true;
        if (_uiWindows.Count == 0) throw new UIException("No UI windows added to the application.");

        MainThreadId = Environment.CurrentManagedThreadId; // ensure main thread id is correct
        NativeWindow.Run();
    }

    /// <summary>
    /// Invokes the given action on the main UI thread. If called from the main thread, the action will be executed
    /// immediately. If called from another thread, the action will be queued and executed on the next frame.
    /// </summary>
    /// <param name="action">The action to invoke on the UI thread.</param>
    public void InvokeOnUI(Action action)
    {
        if (Environment.CurrentManagedThreadId == MainThreadId)
        {
            action();
        }
        else
        {
            _uiActions.Enqueue(action);
        }
    }

    /// <summary>
    /// Disposes the application and releases all resources. This will close the native window and clean up
    /// the ImGui context. After calling this method, the application should not be used anymore.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try
        {
            NativeWindow?.Dispose();
        }
        catch
        {
            // best effort
        }
    }

    private void OnLoad()
    {
        _gl = GL.GetApi(NativeWindow);
        _input = NativeWindow.CreateInput();

        _imguiCtx = ImGui.CreateContext();
        ImGui.SetCurrentContext(_imguiCtx);

        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable | ImGuiConfigFlags.ViewportsEnable |
                          ImGuiConfigFlags.NavEnableKeyboard;

        var mainFontData = LoadFromEmbeddedResource("Inter.ttf");
        if (mainFontData != null)
        {
            io.Fonts.Clear();
            
            unsafe
            {
                fixed (byte* p = mainFontData)
                {
                    io.Fonts.AddFontFromMemoryTTF((IntPtr)p, mainFontData.Length, 16.0f);
                }

                io.Fonts.Build();
            }
        }
        else
        {
            io.Fonts.AddFontDefault();
        }

        VynStyle.Apply();

        _renderer = new ImGuiGlRenderer(_gl!);

        HookInput(_input!, io);
        UpdateIcon();
    }

    private void OnRender(double deltaTime)
    {
        if (_gl is null || _renderer is null) return;
        
        DrainUIActions(); // execute any pending UI actions
        
        ImGui.SetCurrentContext(_imguiCtx);

        var win = NativeWindow.Size;
        var fb = NativeWindow.FramebufferSize;
        if (win.X <= 0 || win.Y <= 0 || fb.X <= 0 || fb.Y <= 0) return;

        var io = ImGui.GetIO();
        io.DisplaySize = new Vector2(win.X, win.Y);
        io.DisplayFramebufferScale = new Vector2(
            win.X != 0 ? (float)fb.X / win.X : 1f,
            win.Y != 0 ? (float)fb.Y / win.Y : 1f
        );
        io.DeltaTime = (float)deltaTime;

        _renderer.NewFrame();
        ImGui.NewFrame();

        DrawChromeAndDockspace();
        DrawAllWindows();
        //TODO here modal render
        Notifications.Render();

        ImGui.Render();
        _renderer.RenderDrawData(ImGui.GetDrawData());

        if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0)
        {
            ImGui.UpdatePlatformWindows();
            ImGui.RenderPlatformWindowsDefault();
        }
    }

    private void DrawChromeAndDockspace()
    {
        var vp = ImGui.GetMainViewport();
        
        ImGui.SetNextWindowPos(vp.WorkPos);
        ImGui.SetNextWindowSize(vp.WorkSize);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        const ImGuiWindowFlags dockFlags = ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoTitleBar |
                                           ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize |
                                           ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoBringToFrontOnFocus |
                                           ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.NoSavedSettings;
        ImGui.Begin("##HostDock", dockFlags);
        var dockId = ImGui.GetID("VynDockSpace");
        ImGui.DockSpace(dockId, Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode);
        ImGui.End();
        ImGui.PopStyleVar(3);
    }

    private void DrawAllWindows()
    {
        foreach (var w in _uiWindows)
        {
            if (ImGui.Begin(w.Title, w.WindowFlags))
            {
                foreach (var layer in w.Layers) layer.Render();
                w.OnUI();
            }

            ImGui.End();
        }
    }

    private void OnResize(Vector2D<int> size)
    {
        _gl?.Viewport(size);
    }

    private void OnClosing()
    {
        _renderer?.Dispose();
        _input?.Dispose();
    }
    
    private void DrainUIActions()
    {
        while (_uiActions.TryDequeue(out var action))
        {
            action();
        }
    }

    private void UpdateIcon()
    {
        if (_icon.HasValue)
        {
            var iconRef = _icon.Value;
            NativeWindow.SetWindowIcon(ref iconRef);
        }
    }

    private void HookInput(IInputContext input, ImGuiIOPtr io)
    {
        foreach (var kb in input.Keyboards)
        {
            kb.KeyDown += (_, k, _) =>
            {
                io.AddKeyEvent(ToKey(k), true);
                SyncMods(kb, io);
            };
            kb.KeyUp += (_, k, _) =>
            {
                io.AddKeyEvent(ToKey(k), false);
                SyncMods(kb, io);
            };
            kb.KeyChar += (_, c) => { io.AddInputCharacter(c); };
        }

        foreach (var m in input.Mice)
        {
            m.MouseDown += (_, b) => io.AddMouseButtonEvent((int)b, true);
            m.MouseUp += (_, b) => io.AddMouseButtonEvent((int)b, false);
            m.MouseMove += (_, p) => io.AddMousePosEvent(p.X, p.Y);
            m.Scroll += (_, w) => io.AddMouseWheelEvent(w.X, w.Y);
        }

        return;

        void SyncMods(IKeyboard k, ImGuiIOPtr pio)
        {
            pio.AddKeyEvent(ImGuiKey.ModCtrl, k.IsKeyPressed(Key.ControlLeft) || k.IsKeyPressed(Key.ControlRight));
            pio.AddKeyEvent(ImGuiKey.ModShift, k.IsKeyPressed(Key.ShiftLeft) || k.IsKeyPressed(Key.ShiftRight));
            pio.AddKeyEvent(ImGuiKey.ModAlt, k.IsKeyPressed(Key.AltLeft) || k.IsKeyPressed(Key.AltRight));
            pio.AddKeyEvent(ImGuiKey.ModSuper, k.IsKeyPressed(Key.SuperLeft) || k.IsKeyPressed(Key.SuperRight));
        }

        ImGuiKey ToKey(Key k) => k switch
        {
            Key.Tab => ImGuiKey.Tab, Key.ShiftLeft or Key.ShiftRight => ImGuiKey.LeftShift,
            Key.ControlLeft or Key.ControlRight => ImGuiKey.LeftCtrl,
            Key.AltLeft or Key.AltRight => ImGuiKey.LeftAlt,
            Key.SuperLeft or Key.SuperRight => ImGuiKey.LeftSuper,
            Key.Up => ImGuiKey.UpArrow, Key.Down => ImGuiKey.DownArrow,
            Key.Left => ImGuiKey.LeftArrow, Key.Right => ImGuiKey.RightArrow,
            Key.Enter => ImGuiKey.Enter, Key.Space => ImGuiKey.Space,
            Key.Backspace => ImGuiKey.Backspace, Key.Delete => ImGuiKey.Delete,
            Key.Home => ImGuiKey.Home, Key.End => ImGuiKey.End,
            Key.PageUp => ImGuiKey.PageUp, Key.PageDown => ImGuiKey.PageDown,
            Key.Escape => ImGuiKey.Escape, _ => 0
        };
    }
    
    private byte[]? LoadFromEmbeddedResource(string resourceName)
    {
        using var stream = typeof(Application).Assembly.GetManifestResourceStream(resourceName);
        if (stream == null) return null;
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }
}