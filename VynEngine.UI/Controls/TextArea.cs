using System.Numerics;
using ImGuiNET;

namespace VynEngine.UI.Controls;

/// <summary>
/// Multi-line text box.
/// </summary>
public sealed class TextArea : Control
{
    /// <summary>Optional label shown above.</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>Current text.</summary>
    public string Text
    {
        get => _text;
        set
        {
            if (_text == value) return;
            _text = value;
            TextChanged?.Invoke(this, _text);
        }
    }

    /// <summary>Requested size; (0,0) uses a default height.</summary>
    public Vector2 Size { get; set; } = new(0, 120);

    /// <summary>If true, input is read-only.</summary>
    public bool ReadOnly { get; set; }

    /// <summary>Raised when the text changes.</summary>
    public event EventHandler<string>? TextChanged;

    private string _text = string.Empty;

    protected override void OnUI()
    {
        var flags = ReadOnly ? ImGuiInputTextFlags.ReadOnly : ImGuiInputTextFlags.None;
        var tmp = _text;
        if (ImGui.InputTextMultiline(Label, ref tmp, 0u, Size, flags) && tmp != _text)
        {
            _text = tmp;
            TextChanged?.Invoke(this, _text);
        }
    }
}