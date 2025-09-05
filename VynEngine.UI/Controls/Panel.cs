using System.Numerics;
using ImGuiNET;

namespace VynEngine.UI.Controls;

/// <summary>
/// A container that renders a scrollable child region and draws its child controls.
/// </summary>
public sealed class Panel : Control
{
    /// <summary>Name/ID of the panel (used as ImGui child identifier).</summary>
    public string Id { get; set; } = "Panel";

    /// <summary>Requested panel size; (0,0) expands to remaining space.</summary>
    public Vector2 Size { get; set; } = new(0, 0);

    /// <summary>Enables horizontal scrolling.</summary>
    public bool ScrollX { get; set; }

    /// <summary>Enables vertical scrolling.</summary>
    public bool ScrollY { get; set; } = true;

    /// <summary>All child controls contained in this panel.</summary>
    public IReadOnlyList<Control> Children => _children;

    private readonly List<Control> _children = [];

    /// <summary>Adds a child control to this panel.</summary>
    public void Add(Control control)
    {
        _children.Add(control);
        control.Layer = Layer;
        control.OnAttached();
    }

    /// <summary>Removes a child control from this panel.</summary>
    public bool Remove(Control control)
    {
        var ok = _children.Remove(control);
        if (ok) control.OnDetached();
        return ok;
    }

    protected override void OnUI()
    {
        var flags = ImGuiChildFlags.None;
        if (ScrollX) flags |= ImGuiChildFlags.AutoResizeX;
        if (ScrollY) flags |= ImGuiChildFlags.AutoResizeY;

        if (ImGui.BeginChild(Id, Size, flags))
        {
            foreach (var c in _children)
                c.Render();
        }
        ImGui.EndChild();
    }
}