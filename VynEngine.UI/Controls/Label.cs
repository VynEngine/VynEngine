using System.Numerics;
using ImGuiNET;

namespace VynEngine.UI.Controls;

/// <summary>
/// Displays a non-interactive text.
/// </summary>
public class Label : Control
{
    /// <summary>Text to display.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Optional text color; if null, ImGui default is used.</summary>
    public Vector4? Color { get; set; }

    /// <summary>Optional wrap width in pixels; null means no wrapping.</summary>
    public float? WrapWidth { get; set; }

    /// <summary>If true, places the next item on the same line.</summary>
    public bool SameLine { get; set; }
    
    protected override void OnUI()
    {
        if (SameLine) ImGui.SameLine();
        if (WrapWidth is { } wrap) ImGui.PushTextWrapPos(ImGui.GetFontSize() * wrap <= 0 ? 0 : wrap);

        if (Color is { } c)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, c);
            ImGui.TextUnformatted(Text);
            ImGui.PopStyleColor();
        }
        else
        {
            ImGui.TextUnformatted(Text);
        }

        if (WrapWidth.HasValue)
        {
            ImGui.PopTextWrapPos();
        }
    }
}