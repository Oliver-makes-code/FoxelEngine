using System.Threading.Tasks;
using Foxel.Common.Content;
using Foxel.Common.Server;

namespace Foxel.Client.Server;

public class IntegratedServer : VoxelServer {
    public IntegratedServer() : base("Integrated Server") {}

    public override async Task Start() {
        ContentDatabase.Instance.Clear();
        ContentDatabase.Instance.LoadPack(MainContentPack.Instance);
        ContentDatabase.Instance.Finish();

        await base.Start();
    }
}
