namespace VynEngine.UI;

/// <summary>
/// A control defines a distinct UI element that can be reused in different layers or windows. It used to generalize
/// behavior and appearance of similar UI elements and abstracts them away from the immediate mode rendering of ImGui.
/// </summary>
public abstract class Control
{
    /// <summary>
    /// Whether this control is visible and should be rendered. It can be used to temporarily hide a control without
    /// removing it from the layer. If false, the control won't be rendered.
    /// </summary>
    public bool Visible { get; set; } = true;
    
    /// <summary>
    /// The layer this control belongs to. This might be null, until the control is added to a layer.
    /// </summary>
    public Layer? Layer { get; internal set; }
    
    /// <summary>
    /// Gets called when the control is added to a layer.
    /// </summary>
    public virtual void OnAttached() {}
    
    /// <summary>
    /// Gets called when the control is removed from a layer. This is not called, when the layer gets overwritten by
    /// another layer. If the control is really being detached, the <see cref="Layer"/> property will keep its value
    /// until this method returns.
    /// </summary>
    public virtual void OnDetached() {}
    
    /// <summary>
    /// Gets called every frame to render the control's UI and listen to user input. This won't be called if
    /// <see cref="Visible"/> is false.
    /// </summary>
    protected abstract void OnUI();
    
    /// <summary>
    /// Renders the control if it is visible by calling <see cref="OnUI"/>.
    /// </summary>
    internal void Render()
    {
        if (Visible)
            OnUI();
    }
}