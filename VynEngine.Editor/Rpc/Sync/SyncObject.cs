namespace VynEngine.Editor.Rpc.Sync;

internal sealed class SyncObject : ISyncNode
{
    /// <summary>
    /// Gets or sets a child node by key. Setting a child to null removes it.
    /// </summary>
    public ISyncNode? this[string key]
    {
        get => _children[key];
        set
        {
            if (value is null)
            {
                Remove(key);
            }
            else
            {
                Set(key, value);
            }
        }
    }

    public event Action<IReadOnlyList<string>, JsonPatchOp>? Changed;
    
    private readonly Dictionary<string, ISyncNode> _children = new(StringComparer.Ordinal);
    private List<string> _path = [];

    /// <summary>
    /// Returns true if the object contains a child with the specified key.
    /// </summary>
    public bool ContainsKey(string key) => _children.ContainsKey(key);
    
    /// <summary>
    /// Sets a child node by key. If a child with the same key already exists, it is replaced.
    /// </summary>
    public SyncObject Set(string key, ISyncNode node)
    {
        _children[key] = node;
        if (node is SyncObject so) so.BindPath(_path, key);
        if (node is SyncList sl) sl.BindPath(_path, key);
        if (node is SyncPropBase pb) pb.BindPath(_path, key);

        node.Changed += Relay;
        Changed?.Invoke(_path, new JsonPatchOp("add", PathString("/" + Escape(key)), node.ToPlain()));

        return this;
    }

    /// <summary>
    /// Removes a child node by key. Returns true if the child was removed, false if it did not exist.
    /// </summary>
    public bool Remove(string key)
    {
        if (!_children.Remove(key, out var node)) return false;
        node.Changed -= Relay;
        Changed?.Invoke(_path, new JsonPatchOp("remove", PathString("/" + Escape(key))));
        return true;
    }

    internal void BindPath(List<string> parent, string key)
    {
        _path = parent.Append(key).ToList();
        foreach (var (k, n) in _children) BindChild(k, n);
    }

    private void BindChild(string key, ISyncNode node)
    {
        if (node is SyncObject so) so.BindPath(_path, key);
        if (node is SyncList sl) sl.BindPath(_path, key);
        if (node is SyncPropBase pb) pb.BindPath(_path, key);
        node.Changed += Relay;
    }

    private void Relay(IReadOnlyList<string> childPath, JsonPatchOp op) => Changed?.Invoke(childPath, op);
    private string PathString(string tail = "") => string.Join('/', _path.Prepend("")).TrimEnd('/') + tail;
    private static string Escape(string s) => s.Replace("~", "~0").Replace("/", "~1");

    public object ToPlain()
    {
        var dict = new Dictionary<string, object?>(_children.Count);
        foreach (var (k, v) in _children) dict[k] = v.ToPlain();
        return dict;
    }
}