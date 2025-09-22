using VynEngine.Editor.Rpc.Sync;

namespace VynEngine.Editor.Rpc.Services;

/// <summary>
/// The sync service is responsible for synchronizing data between the backend and the frontend using the <see cref="Sync.SyncHub"/>.
/// </summary>
[RpcService("sync")]
internal sealed class SyncService
{
    /// <summary>
    /// Requests a snapshot of the sync object with the given key. The snapshot will be sent to the frontend.
    /// </summary>
    [RpcMethod]
    public void RequestSnapshot(string key)
    {
        SyncHub.EmitSnapshot(key);
    }
}