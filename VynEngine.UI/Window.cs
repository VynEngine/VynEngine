using System.Numerics;
using ImGuiNET;

namespace VynEngine.UI;

/// <summary>
/// The UI window controls and manages the ImGui context and its configuration as well as its underlying layers and
/// controls. It is responsible for initializing and shutting down the ImGui context and provides methods to begin
/// and end frames, as well as rendering the UI. Each window has its own ImGui context, allowing for multiple
/// independent UI windows within the same application. It also manages its own OS window and rendering backend.
/// </summary>
public class Window(string title = "Vyn Engine") : IDisposable
{
    public string Title { get; set; } = title;

    /// <summary>
    /// The ImGui window flags that define the behavior and appearance of the main window. The default flags disable
    /// collapsing and enable the menu bar.
    /// </summary>
    public ImGuiWindowFlags WindowFlags { get; set; } = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.MenuBar;
    
    /// <summary>
    /// All layers that are currently attached to this context.
    /// </summary>
    public IReadOnlyList<Layer> Layers => _layers;
    
    /// <summary>
    /// Whether this context has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    private readonly List<Layer> _layers = [];

    /// <summary>
    /// Adds a new layer to this context. The layer will be attached and its <see cref="Layer.OnAttach"/> method
    /// will be called. If the layer is already added to this context, an exception will be thrown.
    /// </summary>
    /// <param name="layer">The layer to add.</param>
    /// <exception cref="InvalidOperationException">The layer is already added to this context.</exception>
    public void AddLayer(Layer layer)
    {
        if (_layers.Contains(layer))
            throw new InvalidOperationException("The layer is already added to this context.");
        
        _layers.Add(layer);
        layer.Attach(this);
    }
    
    /// <summary>
    /// Removes a layer from this context. If the layer was successfully removed, its <see cref="Layer.OnDetach"/>
    /// method will be called. If the layer was not found, nothing will happen.
    /// </summary>
    /// <param name="layer">The layer to remove.</param>
    public void RemoveLayer(Layer layer)
    {
        if (_layers.Remove(layer))
            layer.OnDetach();
    }
    
    /// <summary>
    /// Gets called every frame to render the window's UI. Override this method to implement custom UI rendering
    /// logic. This method is called after all layers have been rendered, so it can be used to render global UI elements
    /// or handle input that is not specific to any layer.
    /// </summary>
    protected internal virtual void OnUI() {}

    /// <summary>
    /// Disposes the ImGui context and releases all resources. After calling this method, the context should not
    /// be used anymore. <see cref="IsDisposed"/> indicates whether the context has been disposed.
    /// </summary>
    public void Dispose()
    {
        if (IsDisposed) return;
        IsDisposed = true;
        
        foreach (var layer in _layers)
        {
            layer.OnDetach();
        }
        
        _layers.Clear();
        
        GC.SuppressFinalize(this);
    }
}