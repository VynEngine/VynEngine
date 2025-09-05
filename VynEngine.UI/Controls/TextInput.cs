using ImGuiNET;

namespace VynEngine.UI.Controls;

/// <summary>
/// Single-line text input field.
/// </summary>
public sealed class TextInput : Control
{
    /// <summary>Optional label shown to the left.</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>Current text value.</summary>
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

    /// <summary>Hint text shown when empty; requires ImGui 1.89+ (InputTextWithHint).</summary>
    public string Hint { get; set; } = string.Empty;

    /// <summary>If true, input is read-only.</summary>
    public bool ReadOnly { get; set; }

    /// <summary>Max character count; 0 means unlimited.</summary>
    public int MaxLength { get; set; }

    /// <summary>Raised when the text changes.</summary>
    public event EventHandler<string>? TextChanged;

    /// <summary>Raised when the user confirms with Enter.</summary>
    public event EventHandler<string>? Submitted;

    private string _text = string.Empty;

    protected override void OnUI()
    {
        var flags = ImGuiInputTextFlags.EnterReturnsTrue | (ReadOnly ? ImGuiInputTextFlags.ReadOnly : 0);
        var tmp = _text;

        var used = !string.IsNullOrEmpty(Hint)
            ? ImGui.InputTextWithHint(Label, Hint, ref tmp, (uint)Math.Max(MaxLength, 0), flags)
            : ImGui.InputText(Label, ref tmp, (uint)Math.Max(MaxLength, 0), flags);

        if (!ReferenceEquals(tmp, _text) && tmp != _text)
        {
            _text = tmp;
            TextChanged?.Invoke(this, _text);
        }

        if (used)
            Submitted?.Invoke(this, _text);
    }
}