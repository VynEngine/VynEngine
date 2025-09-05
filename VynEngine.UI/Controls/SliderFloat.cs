using ImGuiNET;

namespace VynEngine.UI.Controls;

/// <summary>
/// Horizontal float slider.
/// </summary>
public sealed class SliderFloat : Control
{
    /// <summary>Optional label displayed to the left.</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>Current slider value.</summary>
    public float Value
    {
        get => _value;
        set
        {
            if (Math.Abs(_value - value) < float.Epsilon) return;
            _value = value;
            Changed?.Invoke(this, _value);
        }
    }

    /// <summary>Lower bound.</summary>
    public float Min { get; set; } = 0f;

    /// <summary>Upper bound.</summary>
    public float Max { get; set; } = 1f;

    /// <summary>Optional display format, e.g. "%.3f".</summary>
    public string Format { get; set; } = "%.3f";

    /// <summary>Raised when the value changes.</summary>
    public event EventHandler<float>? Changed;

    private float _value;

    protected override void OnUI()
    {
        var v = _value;
        if (ImGui.SliderFloat(Label, ref v, Min, Max, Format) && Math.Abs(v - _value) > float.Epsilon)
        {
            _value = v;
            Changed?.Invoke(this, _value);
        }
    }
}