using ImGuiNET;

namespace VynEngine.UI.Controls;

/// <summary>
/// Dropdown selection list.
/// </summary>
public sealed class ComboBox : Control
{
    /// <summary>Label displayed to the left of the combo box.</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>Items to show in the dropdown.</summary>
    public IList<string> Items { get; set; } = new List<string>();

    /// <summary>Current selected index; -1 means none.</summary>
    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (_selectedIndex == value) return;
            _selectedIndex = value;
            Changed?.Invoke(this, _selectedIndex);
        }
    }

    /// <summary>Raised when the selected index changes.</summary>
    public event EventHandler<int>? Changed;

    private int _selectedIndex;

    protected override void OnUI()
    {
        var preview = _selectedIndex >= 0 && _selectedIndex < Items.Count ? Items[_selectedIndex] : "(none)";
        if (ImGui.BeginCombo(Label, preview))
        {
            for (var i = 0; i < Items.Count; i++)
            {
                var selected = i == _selectedIndex;
                if (ImGui.Selectable(Items[i], selected))
                {
                    _selectedIndex = i;
                    Changed?.Invoke(this, _selectedIndex);
                }
                if (selected) ImGui.SetItemDefaultFocus();
            }
            ImGui.EndCombo();
        }
    }
}