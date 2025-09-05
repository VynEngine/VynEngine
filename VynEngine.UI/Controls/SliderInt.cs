using ImGuiNET;

namespace VynEngine.UI.Controls;

/// <summary>
/// Horizontal integer slider.
/// </summary>
public sealed class SliderInt : Control
{
    /// <summary>Optional label displayed to the left.</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>Current slider value.</summary>
    public int Value
    {
        get => _value;
        set
        {
            if (_value == value) return;
            _value = value;
            Changed?.Invoke(this, _value);
        }
    }

    /// <summary>Lower bound.</summary>
    public int Min { get; set; } = 0;

    /// <summary>Upper bound.</summary>
    public int Max { get; set; } = 100;

    /// <summary>Raised when the value changes.</summary>
    public event EventHandler<int>? Changed;

    private int _value;
    
    protected override void OnUI()
    {
        var v = _value;
        if (ImGui.SliderInt(Label, ref v, Min, Max) && v != _value)
        {
            _value = v;
            Changed?.Invoke(this, _value);
        }
    }
}