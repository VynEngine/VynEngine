namespace VynEngine.UI;

/// <summary>
/// A layer defines a distinct section of the UI that is rendered on the imaginary z-axis. It can be used to
/// seperate different overlaying parts like dialogs, menus or toolbars.
/// </summary>
public abstract class Layer
{
    /// <summary>
    /// The UI context this layer belongs to. This might be null, until the layer is attached to a window.
    /// </summary>
    protected Window? UI { get; private set; }
    
    /// <summary>
    /// Whether this layer is visible and should be rendered. It can be used to temporarily hide a layer without
    /// removing it from the window. If false, <see cref="OnUI"/> won't be called.
    /// </summary>
    public bool Visible { get; set; } = true;
    
    /// <summary>
    /// All controls that are currently added to this layer.
    /// </summary>
    public IReadOnlyList<Control> Controls => _controls;
    
    private readonly List<Control> _controls = [];
    
    /// <summary>
    /// Adds a new control to this layer. The control's <see cref="Control.OnAttached"/> method will be called.
    /// If the control is already added to this layer, an exception will be thrown.
    /// </summary>
    /// <param name="control">The control to add.</param>
    /// <exception cref="InvalidOperationException">The control is already added to this layer.</exception>
    public void AddControl(Control control)
    {
        if (_controls.Contains(control))
            throw new InvalidOperationException("The control is already added to this layer.");
        
        _controls.Add(control);
        control.Layer = this;
        control.OnAttached();
    }
    
    /// <summary>
    /// Removes a control from this layer. The control's <see cref="Control.OnDetached"/> method will be called.
    /// If the control was not part of this layer, nothing will happen.
    /// </summary>
    /// <param name="control">The control to remove.</param>
    public void RemoveControl(Control control)
    {
        if (_controls.Remove(control))
        {
            control.OnDetached();
            control.Layer = null;
        }
    }
    
    /// <summary>
    /// Gets called, when the layer is attached to its window.
    /// </summary>
    protected virtual void OnAttach() {}
    
    /// <summary>
    /// Gets called, when the layer is detached from its window.
    /// </summary>
    public virtual void OnDetach() {}
    
    /// <summary>
    /// Gets called every frame to render the layer's UI.
    /// </summary>
    protected virtual void OnUI() {}
    
    /// <summary>
    /// Internal helper to set the UI context this layer belongs to and call <see cref="OnAttach"/>.
    /// </summary>
    internal void Attach(Window ui)
    {
        UI = ui;
        OnAttach();
    }
    
    /// <summary>
    /// Internal helper to call <see cref="OnUI"/> if <see cref="Visible"/> is true.
    /// </summary>
    internal void Render()
    {
        if (Visible)
        {
            OnUI();
            
            foreach (var control in _controls)
            {
                control.Render();
            }
        }
    }
}