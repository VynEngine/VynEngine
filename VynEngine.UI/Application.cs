using System.Numerics;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace VynEngine.UI;

public sealed class Application(string title = "VynEngine") : IDisposable
{
    public Vector2 InitialSize { get; set; } = new(1600, 900);

    public TitleBarChrome Chrome { get; } = new()
    {
        Title = title
    };

    private readonly List<Window> _uiWindows = [];
    private IWindow? _native;
    private GL? _gl;
    private IInputContext? _input;
    private ImGuiGlRenderer? _renderer;

    private IntPtr _imguiCtx;
    private bool _started, _disposed;

    public void AddWindow(Window uiWindow) => _uiWindows.Add(uiWindow);

    public void Start()
    {
        if (_started) return;
        _started = true;
        if (_uiWindows.Count == 0) throw new InvalidOperationException("No UI windows added to the application.");
        
        var opts = WindowOptions.Default with
        {
            Title = Chrome.Title,
            Size = new Vector2D<int>((int)InitialSize.X, (int)InitialSize.Y),
            API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.ForwardCompatible,
                new APIVersion(3, 3)),
            VSync = true,
            IsEventDriven = false
        };

        _native = Silk.NET.Windowing.Window.Create(opts);
        _native.Load += OnLoad;
        _native.Render += OnRender;
        _native.Resize += OnResize;
        _native.Closing += OnClosing;

        _native.Run();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try
        {
            _native?.Dispose();
        }
        catch
        {
            // best effort
        }
    }

    private void OnLoad()
    {
        _gl = GL.GetApi(_native!);
        _input = _native!.CreateInput();

        _imguiCtx = ImGui.CreateContext();
        ImGui.SetCurrentContext(_imguiCtx);

        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable | ImGuiConfigFlags.ViewportsEnable |
                          ImGuiConfigFlags.NavEnableKeyboard;

        io.Fonts.AddFontDefault();

        var style = ImGui.GetStyle();
        style.WindowRounding = 6f;
        style.Colors[(int)ImGuiCol.WindowBg].W = 1.0f;

        _renderer = new ImGuiGlRenderer(_gl!);

        HookInput(_input!, io);
    }

    private void OnRender(double deltaTime)
    {
        if (_gl is null || _renderer is null) return;
        ImGui.SetCurrentContext(_imguiCtx);

        var win = _native!.Size;
        var fb = _native.FramebufferSize;
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
                                           ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.NoSavedSettings |
                                           ImGuiWindowFlags.MenuBar;
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
        if (_imguiCtx != IntPtr.Zero)
        {
            ImGui.DestroyContext(_imguiCtx);
            _imguiCtx = IntPtr.Zero;
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
}