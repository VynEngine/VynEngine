using ImGuiNET;

namespace VynEngine.UI.Controls;

/// <summary>
/// Binary toggle (checkbox) with a label.
/// </summary>
public sealed class CheckBox : Control
{
    /// <summary>Displayed label next to the box.</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>Current state of the checkbox.</summary>
    public bool Value
    {
        get => _value;
        set
        {
            if (_value == value) return;
            _value = value;
            Changed?.Invoke(this, value);
        }
    }

    /// <summary>Raised when <see cref="Value"/> changes.</summary>
    public event EventHandler<bool>? Changed;

    private bool _value;
    
    protected override void OnUI()
    {
        var v = _value;
        if (ImGui.Checkbox(Label, ref v) && v != _value)
        {
            _value = v;
            Changed?.Invoke(this, _value);
        }
    }
}