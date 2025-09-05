using System.Numerics;
using ImGuiNET;

namespace VynEngine.UI.Controls;

/// <summary>
/// Displays an image from a renderer-provided texture.
/// </summary>
public sealed class Image : Control
{
    /// <summary>Renderer-specific texture identifier (e.g., OpenGL handle).</summary>
    public IntPtr TextureId { get; set; } = IntPtr.Zero;

    /// <summary>Image size in pixels.</summary>
    public Vector2 Size { get; set; } = new(64, 64);

    /// <summary>UV region min (top-left).</summary>
    public Vector2 UV0 { get; set; } = new(0, 0);

    /// <summary>UV region max (bottom-right).</summary>
    public Vector2 UV1 { get; set; } = new(1, 1);

    /// <summary>Tint color multiplied with the image.</summary>
    public Vector4 TintColor { get; set; } = Vector4.One;

    /// <summary>Optional border color; alpha 0 disables border.</summary>
    public Vector4 BorderColor { get; set; } = Vector4.Zero;

    protected override void OnUI()
    {
        ImGui.Image(TextureId, Size, UV0, UV1, TintColor, BorderColor);
    }
}