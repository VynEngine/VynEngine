namespace VynEngine.Editor.Rpc.Sync;

/// <summary>
/// A sync node is a node in the sync tree like a property, object or array which can sync its state to the frontend.
/// </summary>
internal interface ISyncNode
{
    /// <summary>
    /// Gets called when the node or one of its children changes.
    /// </summary>
    event Action<IReadOnlyList<string>, JsonPatchOp>? Changed;

    /// <summary>
    /// Converts the node into a plain C# object.
    /// </summary>
    object? ToPlain();
}