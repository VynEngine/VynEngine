using ImGuiNET;

namespace VynEngine.UI.Controls;

/// <summary>
/// Visual separator line.
/// </summary>
public sealed class Separator : Control
{
    /// <summary>
    /// Optional label displayed in the middle of the separator line.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    protected override void OnUI()
    {
        if (string.IsNullOrEmpty(Label))
            ImGui.Separator();
        else
            ImGui.SeparatorText(Label);
    }
}