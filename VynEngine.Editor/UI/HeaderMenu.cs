using VynEngine.Editor.Rpc.Sync;

namespace VynEngine.Editor.UI;

/// <summary>
/// The header menu defines an item which is added to the header menu bar of the application.
/// </summary>
public sealed class HeaderMenu
{
    /// <summary>
    /// The internal synchronization handle for the menu item.
    /// </summary>
    internal SyncObject Handle { get; } = new();

    /// <summary>
    /// The display label for the menu item.
    /// </summary>
    public string Label 
    {
        get => _labelProp.Value;
        set => _labelProp.Value = value;
    }
    
    /// <summary>
    /// The optional hotkey for the menu item.
    /// </summary>
    public string? Hotkey 
    {
        get => _hotkeyProp.Value;
        set => _hotkeyProp.Value = value;
    }
    
    /// <summary>
    /// The optional icon for the menu item. Uses fontawesome 6.
    /// </summary>
    public string? Icon 
    {
        get => _iconProp.Value;
        set => _iconProp.Value = value;
    }
    
    /// <summary>
    /// The optional icon color for the menu item.
    /// </summary>
    public string? IconColor 
    {
        get => _iconColorProp.Value;
        set => _iconColorProp.Value = value;
    }
    
    /// <summary>
    /// Whether the menu item is disabled.
    /// </summary>
    public bool Disabled 
    {
        get => _disabledProp.Value;
        set => _disabledProp.Value = value;
    }
    
    /// <summary>
    /// The list of child menu items for creating submenus.
    /// </summary>
    public IReadOnlyList<HeaderMenu> Children => _children;
    
    /// <summary>
    /// The unique identifier for the menu item.
    /// </summary>
    private readonly string _id;

    /// <summary>
    /// A list of child menu items for creating submenus.
    /// </summary>
    private readonly List<HeaderMenu> _children = [];

    private readonly SyncProp<string> _labelProp;
    private readonly SyncProp<string?> _hotkeyProp;
    private readonly SyncProp<string?> _iconProp;
    private readonly SyncProp<string?> _iconColorProp;
    private readonly SyncProp<bool> _disabledProp;
    private readonly SyncList _childrenSync;
    
    /// <summary>
    /// Event invoked when the menu item is clicked.
    /// </summary>
    private event Action? _onClicked;

    private HeaderMenu(string id, string? label = null, string? hotkey = null, string? icon = null, string? iconColor = null, bool disabled = false)
    {
        _id = id;
        _labelProp = new SyncProp<string>(label ?? "");
        _hotkeyProp = new SyncProp<string?>(hotkey);
        _iconProp = new SyncProp<string?>(icon);
        _iconColorProp = new SyncProp<string?>(iconColor);
        _disabledProp = new SyncProp<bool>(disabled);
        _childrenSync = new SyncList();
        
        Handle.Set("id", new SyncProp<string>(_id));
        Handle.Set("label", _labelProp);
        Handle.Set("hotkey", _hotkeyProp);
        Handle.Set("icon", _iconProp);
        Handle.Set("iconColor", _iconColorProp);
        Handle.Set("disabled", _disabledProp);
        Handle.Set("children", _childrenSync);
    }
    
    /// <summary>
    /// Hooks a callback to be invoked when the menu item is clicked.
    /// </summary>
    public HeaderMenu On(Action callback)
    {
        _onClicked += callback;
        return this;
    }
    
    /// <summary>
    /// Adds multiple child menu items to create submenus.
    /// </summary>
    /// <param name="children">The child menu items to add.</param>
    /// <returns>>The current <see cref="HeaderMenu"/> instance for chaining.</returns
    public HeaderMenu AddChildren(params HeaderMenu[] children)
    {
        foreach (var child in children)
        {
            AddChild(child);
        }
        return this;
    }
    
    /// <summary>
    /// Adds a child menu item to create a submenu.
    /// </summary>
    /// <param name="child">The child menu item to add.</param>
    /// <returns>>The current <see cref="HeaderMenu"/> instance for chaining.</returns>
    public HeaderMenu AddChild(HeaderMenu child)
    {
        _children.Add(child);
        _childrenSync.Add(child.Handle);
        return this;
    }

    /// <summary>
    /// Removes a child menu item from the submenu.
    /// </summary>
    /// <param name="child">The child menu item to remove.</param>
    /// <returns>>The current <see cref="HeaderMenu"/> instance for chaining.</returns>
    public HeaderMenu RemoveChild(HeaderMenu child)
    {
        if (_children.Remove(child))
        {
            for (var i = 0; i < _childrenSync.Count; i++)
            {
                if (ReferenceEquals(_childrenSync[i], child.Handle))
                {
                    _childrenSync.RemoveAt(i);
                    break;
                }
            }
        }
        return this;
    }

    /// <summary>
    /// Internal method to invoke the click event for the menu item.
    /// </summary>
    internal void InvokeClicked(string id)
    {
        if (id == _id)
        {
            _onClicked?.Invoke();
        }
        else
        {
            foreach (var child in _children)
            {
                child.InvokeClicked(id);
            }
        }
    }

    /// <summary>
    /// Creates a new header menu item with the specified properties.
    /// </summary>
    /// <param name="label">The display label for the menu item.</param>
    /// <param name="hotkey">The optional hotkey for the menu item.</param>
    /// <param name="icon">The optional icon for the menu item. Uses fontawesome 6.</param>
    /// <param name="iconColor">The optional icon color for the menu item.</param>
    /// <param name="disabled">Whether the menu item is disabled.</param>
    /// <returns>>A new instance of <see cref="HeaderMenu"/> representing the menu item.</returns>
    public static HeaderMenu Item(string label, string? hotkey = null, string? icon = null, string? iconColor = null, bool disabled = false)
    {
        var item = new HeaderMenu(label + "_item_" + Guid.NewGuid().ToString("N"), label, hotkey, icon, iconColor, disabled);
        item.Handle.Set("type", new SyncProp<string>("item"));
        return item;
    }
    
    /// <summary>
    /// Creates a new separator menu item.
    /// </summary>
    /// <returns>A new instance of <see cref="HeaderMenu"/> representing the separator.</returns>
    public static HeaderMenu Separator()
    {
        var item = new HeaderMenu("separator_item_" + Guid.NewGuid().ToString("N"));
        item.Handle.Set("type", new SyncProp<string>("separator"));
        return item;
    }
}