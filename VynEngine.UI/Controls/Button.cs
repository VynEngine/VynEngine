using System.Numerics;
using ImGuiNET;

namespace VynEngine.UI.Controls;

/// <summary>
/// Clickable button control.
/// </summary>
public sealed class Button : Control
{
    /// <summary>Button label text.</summary>
    public string Text { get; set; } = "Button";

    /// <summary>Requested size; (0,0) lets ImGui auto-size.</summary>
    public Vector2 Size { get; set; } = Vector2.Zero;

    /// <summary>Tool tip text shown when hovering; empty to disable.</summary>
    public string? ToolTip { get; set; }

    /// <summary>Raised when the button is clicked.</summary>
    public event EventHandler? Clicked;

    protected override void OnUI()
    {
        if (ImGui.Button(Text, Size))
            Clicked?.Invoke(this, EventArgs.Empty);

        if (!string.IsNullOrEmpty(ToolTip) && ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal))
            ImGui.SetTooltip(ToolTip);
    }
}