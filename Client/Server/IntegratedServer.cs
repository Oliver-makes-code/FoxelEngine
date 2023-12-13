using Voxel.Common.Content;
using Voxel.Common.Server;

namespace Voxel.Client.Server;

public class IntegratedServer : VoxelServer {


    public override void Start() {
        ContentDatabase.Instance.Clear();
        ContentDatabase.Instance.LoadPack(MainContentPack.Instance);
        ContentDatabase.Instance.Finish();

        base.Start();
    }
}
