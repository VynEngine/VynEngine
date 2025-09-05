using System.Numerics;
using ImGuiNET;

namespace VynEngine.UI.Controls;

/// <summary>
/// Visual progress indicator.
/// </summary>
public sealed class ProgressBar : Control
{
    /// <summary>Progress from 0.0 to 1.0.</summary>
    public float Fraction { get; set; }

    /// <summary>Optional overlay text; null to let ImGui show percentage.</summary>
    public string? Overlay { get; set; }

    /// <summary>Requested size; (0,0) uses default width/height.</summary>
    public Vector2 Size { get; set; } = new(0, 0);

    protected override void OnUI()
    {
        ImGui.ProgressBar(Math.Clamp(Fraction, 0f, 1f), Size, Overlay);
    }
}