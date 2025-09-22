namespace VynEngine.Editor.Rpc.Sync;

/// <summary>
/// A sync list is a list of <see cref="ISyncNode"/>s which can sync its state to the frontend.
/// </summary>
internal sealed class SyncList : ISyncNode
{
    /// <summary>
    /// The number of items in the list.
    /// </summary>
    public int Count => _items.Count;
    
    /// <summary>
    /// Gets or sets the item at the specified index.
    /// Setting an item to null removes it from the list.
    /// Setting an item at an index equal to Count or greater adds it to the end of the list.
    /// Setting an item at an index less than Count replaces the item at that index.
    /// </summary>
    public ISyncNode? this[int index]
    {
        get => _items[index];
        set
        {
            if (!ReferenceEquals(_items[index], value))
            {
                _items[index].Changed -= Relay;
                
                if (value == null)
                {
                    _items.RemoveAt(index);
                    Changed?.Invoke(_path, new JsonPatchOp("remove", PathString($"/{index}")));
                }
                else if (_items.Count >= index)
                {
                    _items.Add(value);
                    BindChild(index, value);
                    Changed?.Invoke(_path, new JsonPatchOp("add", PathString($"/{index}"), value.ToPlain()));
                }
                else // Replace
                {
                    _items[index] = value;
                    BindChild(index, value);
                    Changed?.Invoke(_path, new JsonPatchOp("replace", PathString($"/{index}"), value.ToPlain()));
                }
            }
        }
    }
    
    public event Action<IReadOnlyList<string>, JsonPatchOp>? Changed;
    
    private readonly List<ISyncNode> _items = [];
    private List<string> _path = [];

    /// <summary>
    /// Adds a new item to the end of the list.
    /// </summary>
    public void Add(ISyncNode node)
    {
        _items.Add(node);
        BindChild(_items.Count - 1, node);
        Changed?.Invoke(_path, new JsonPatchOp("add", PathString($"/{_items.Count-1}"), node.ToPlain()));
    }
    
    /// <summary>
    /// Removes the item at the specified index from the list.
    /// </summary>
    public void RemoveAt(int index)
    {
        _items[index].Changed -= Relay;
        _items.RemoveAt(index);
        Changed?.Invoke(_path, new JsonPatchOp("remove", PathString($"/{index}")));
    }

    internal void BindPath(List<string> parent, string key)
    {
        _path = parent.Append(key).ToList();
        
        for (var i = 0; i < _items.Count; i++)
        {
            BindChild(i, _items[i]);
        }
    }

    private void BindChild(int idx, ISyncNode node)
    {
        if (node is SyncObject so) so.BindPath(_path, idx.ToString());
        if (node is SyncList sl)   sl.BindPath(_path, idx.ToString());
        if (node is SyncPropBase pb) pb.BindPath(_path, idx.ToString());
        node.Changed += Relay;
    }

    private void Relay(IReadOnlyList<string> p, JsonPatchOp op) => Changed?.Invoke(p, op);
    
    private string PathString(string tail = "") => string.Join('/', _path.Prepend("")).TrimEnd('/') + tail;

    public object ToPlain() => _items.Select(i => i.ToPlain()).ToList();
}