namespace VynEngine.Editor.Rpc;

/// <summary>
/// The attribute to mark a class as an RPC service which can be called from the JavaScript side.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
internal sealed class RpcServiceAttribute(string name) : Attribute
{
    /// <summary>
    /// The name of the service. This is the name that will be used to call the service from JavaScript.
    /// </summary>
    public string Name { get; } = name;
}