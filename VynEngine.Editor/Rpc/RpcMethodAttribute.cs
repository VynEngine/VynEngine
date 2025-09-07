namespace VynEngine.Editor.Rpc;

/// <summary>
/// The attribute to mark a method as an RPC method which can be called from the JavaScript side.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
internal sealed class RpcMethodAttribute(string? name = null) : Attribute
{
    /// <summary>
    /// The name of the method. If null, the method name will be used.
    /// </summary>
    public string? Name { get; } = name;
}