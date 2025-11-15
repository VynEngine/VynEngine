using VynEngine.Editor.Rpc.Sync;

namespace VynEngine.Editor.UI;

/// <summary>
/// The header is a UI component that represents the top section of the application window, typically containing the title bar and menu items.
/// </summary>
public class Header
{
    /// <summary>
    /// Sets the subtitle text displayed in the header. If set to null or an empty string, the subtitle will be reset.
    /// </summary>
    public string? Subtitle
    {
        set => Program.Emit("window", "updateSubtitle", value ?? "");
    }

    /// <summary>
    /// The "File" menu in the header menu bar.
    /// </summary>
    public HeaderMenu FileMenu { get; } = HeaderMenu.Item("File").AddChildren(
        HeaderMenu.Item("New").On(() => Console.WriteLine("New File clicked")),
        HeaderMenu.Item("Open"),
        HeaderMenu.Item("Save"),
        HeaderMenu.Item("Save As"),
        HeaderMenu.Item("Exit")
    );
    /// <summary>
    /// The "Edit" menu in the header menu bar.
    /// </summary>
    public HeaderMenu EditMenu { get; } = HeaderMenu.Item("Edit");
    /// <summary>
    /// The "View" menu in the header menu bar.
    /// </summary>
    public HeaderMenu ViewMenu { get; } = HeaderMenu.Item("View");
    /// <summary>
    /// The "Help" menu in the header menu bar.
    /// </summary>
    public HeaderMenu HelpMenu { get; } = HeaderMenu.Item("Help");

    private readonly SyncList _headerMenus = new();

    internal Header()
    {
        _headerMenus.Add(FileMenu.Handle);
        _headerMenus.Add(EditMenu.Handle);
        _headerMenus.Add(ViewMenu.Handle);
        _headerMenus.Add(HelpMenu.Handle);
        SyncHub.Register("header.menus", new SyncObject().Set("items", _headerMenus));
    }
}