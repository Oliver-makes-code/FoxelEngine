using NLog;
using Voxel.Client;
using Voxel.Server;

namespace Voxel.Common;

public static class LogUtil {
    public static Logger PlatformLogger {
        get => Init.IsClient ? VoxelClient.Log : VoxelServer.Log;
    }
}