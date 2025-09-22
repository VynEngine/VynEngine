namespace VynEngine.Editor.Rpc.Sync;

/// <summary>
/// A sync prop is a property that can sync its state to the frontend.
/// </summary>
/// <typeparam name="T">The type of the property.</typeparam>
internal sealed class SyncProp<T> : SyncPropBase
{
    /// <summary>
    /// The value of the property.
    /// </summary>
    public T Value
    {
        get => _value;
        set {
            if (!Equals(_value, value)) {
                _value = value;
                Emit(new JsonPatchOp("replace", PathString(), _value));
            }
        }
    }

    private T _value;

    /// <summary>
    /// Creates a new sync property.
    /// </summary>
    /// <param name="initial">The initial value of the property.</param>
    /// <param name="path">The path of the property in the sync tree. This is set automatically when the property is added to a SyncObject or SyncArray.</param>
    internal SyncProp(T initial = default!, List<string>? path = null)
    {
        _value = initial;
        _path = path ?? [];
    }

    public override object ToPlain() => _value!;

    private string PathString(string tail = "") => string.Join('/', _path.Prepend("")).TrimEnd('/') + tail;
}