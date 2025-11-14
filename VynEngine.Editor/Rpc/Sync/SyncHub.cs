namespace VynEngine.Editor.Rpc.Sync;

internal static class SyncHub
{
    private static readonly Dictionary<string,(SyncObject root,long ver)> _objs = new();

    /// <summary>
    /// Registers the given <see cref="SyncObject"/> with the given key.
    /// </summary>
    /// <param name="key">The key is the identifier for the object. It must be unique.</param>
    /// <param name="root">The root <see cref="SyncObject"/> to register.</param>
    public static SyncObject Register(string key, SyncObject root)
    {
        _objs[key] = (root, 1);
        root.Changed += (path, op) =>
        {
            var valueTuple = _objs[key];
            EmitPatch(key, ++valueTuple.ver, op);
            _objs[key] = valueTuple;
        };
        return root;
    }
    
    /// <summary>
    /// Requests a snapshot of the object with the given key. It will be emitted to the frontend.
    /// </summary>
    /// <param name="key">The key of the object to request a snapshot of.</param>
    public static void EmitSnapshot(string key)
    {
        if (!_objs.ContainsKey(key)) return;
        var (ver, data) = GetSnapshot(key);
        Program.Emit("sync", "snapshot", new { key, version=ver, data });
    }

    private static void EmitPatch(string key, long version, JsonPatchOp op)
    {
        Program.Emit("sync", "patch", new
        {
            key, version, ops = new[]{ new { op=op.Op, path=op.Path, value=op.Value } }
        });
    }

    private static (long ver, object data) GetSnapshot(string key)
    {
        var (root, ver) = _objs[key];
        return (ver, root.ToPlain()!);
    }
}