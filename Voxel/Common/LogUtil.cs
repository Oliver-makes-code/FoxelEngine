using NLog;
using Voxel.Client;
using Voxel.Server;
using Voxel.Test;

namespace Voxel.Common;

public static class LogUtil {
    public static Logger PlatformLogger {
        get =>
            Init.platform == Platform.Test ?
                TestSuite.Log : 
                Init.platform == Platform.Client ? 
                    VoxelClient.Log : 
                    VoxelServer.Log;
    }
}
