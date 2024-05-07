using System;
using System.Threading.Tasks;
using Veldrid;
using Voxel.Core;
using Voxel.Core.Assets;
using Voxel.Core.Rendering;

namespace Voxel.Client.Rendering;

public class ReloadableDependency<T> {
    public delegate Task<T> ValueCreator(PackManager packs, RenderSystem renderSystem, Framebuffer buffer);

    public readonly ValueCreator Creator;

    public readonly PackManager.ReloadTask ReloadTask;

    public readonly VoxelClient Client;

    public T? value { get; private set; }

    public ReloadableDependency(ValueCreator creator, VoxelClient client) {
        Creator = creator;

        Client = client;

        ReloadTask = PackManager.RegisterResourceLoader(AssetType.Assets, Reload);
    }

    public async Task Reload(PackManager packs) {
        await Client.gameRenderer!.FrameBufferTask;
        await Client.renderSystem!.ShaderManager.ReloadTask;
        try {
            value = await Creator(packs, Client.renderSystem!, Client.gameRenderer!.frameBuffer!.Framebuffer);
        } catch (Exception e) {
            Game.Logger.Error(e);
        }
    }
}
