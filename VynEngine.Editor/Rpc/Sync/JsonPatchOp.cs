namespace VynEngine.Editor.Rpc.Sync;

/// <summary>
/// A JSON patch operation.
/// </summary>
/// <param name="Op">The operation type (e.g. "add", "remove", "replace").</param>
/// <param name="Path">The JSON pointer path to the target location.</param>
/// <param name="Value">The value to be used in the operation (if applicable).</param>
internal record JsonPatchOp(string Op, string Path, object? Value = null);