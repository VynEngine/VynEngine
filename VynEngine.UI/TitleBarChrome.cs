namespace VynEngine.UI;

/// <summary>
/// The title bar chrome is a custom title bar implementation for ImGui windows.
/// </summary>
public sealed class TitleBarChrome
{
    /// <summary>
    /// The title text displayed in the title bar.
    /// </summary>
    public string Title { get; set; } = "Vyn Engine";

    /// <summary>
    /// The subtitle text displayed in the title bar. If null, no subtitle will be shown.
    /// </summary>
    public string? Subtitle { get; set; }
    
    /// <summary>
    /// Whether the title bar chrome is enabled and should be rendered. If false, the default ImGui title bar will be used.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// The height of the title bar in pixels. Default is 36 pixels.
    /// </summary>
    public float Height { get; set; } = 36f;

    /// <summary>
    /// The title text displayed in the title bar.
    /// </summary>
    public IntPtr IconTextureId { get; set; } = IntPtr.Zero;

    /// <summary>
    /// Whether to show the minimize button.
    /// </summary>
    public bool ShowMin { get; set; } = true;

    /// <summary>
    /// Whether to show the maximize button.
    /// </summary>
    public bool ShowMax { get; set; } = true;

    /// <summary>
    /// Whether to show the close button.
    /// </summary>
    public bool ShowClose { get; set; } = true;

    /// <summary>
    /// Whether dragging the title bar is enabled.
    /// </summary>
    public bool EnableDrag { get; set; } = true;

    /// <summary>
    /// Whether resizing the window by dragging the edges is enabled.
    /// </summary>
    public bool EnableResize { get; set; } = true;
    
    /// <summary>
    /// An optional action that gets called to render a custom menu in the title bar. If null, no menu will be shown.
    /// </summary>
    public Action? RenderMenu { get; set; }
}