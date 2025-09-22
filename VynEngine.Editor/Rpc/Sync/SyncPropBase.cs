namespace VynEngine.Editor.Rpc.Sync;

/// <summary>
/// The sync prop base is the base class of the <see cref="SyncProp{T}"/> class but without a generic type parameter.
/// </summary>
internal abstract class SyncPropBase : ISyncNode
{
    public event Action<IReadOnlyList<string>, JsonPatchOp>? Changed;
    
    protected List<string> _path = [];
    
    protected void Emit(JsonPatchOp op) => Changed?.Invoke(_path, op);
    
    internal void BindPath(List<string> parent, string key) => _path = parent.Append(key).ToList();
    
    public abstract object? ToPlain();
}