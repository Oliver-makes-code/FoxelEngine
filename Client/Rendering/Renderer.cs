using System;
using Veldrid;
using Voxel.Core.Rendering;

namespace Voxel.Client.Rendering;

public abstract class Renderer : IDisposable {
    public readonly VoxelClient Client;
    public readonly RenderSystem RenderSystem;
    public readonly ResourceFactory ResourceFactory;

    public CommandList CommandList => RenderSystem.MainCommandList;

    public Renderer(VoxelClient client) {
        Client = client;
        RenderSystem = client.RenderSystem;
        ResourceFactory = RenderSystem.ResourceFactory;
    }

    public abstract void Render(double delta);

    public void SetPipeline(Pipeline pipeline) {
        RenderSystem.LastPipeline = RenderSystem.CurrentPipeline;
        RenderSystem.CurrentPipeline = pipeline;

        CommandList.SetPipeline(pipeline);
    }

    public void RestoreLastPipeline() {
        RenderSystem.CurrentPipeline = RenderSystem.LastPipeline;
        RenderSystem.LastPipeline = null;

        if (RenderSystem.CurrentPipeline != null)
            CommandList.SetPipeline(RenderSystem.CurrentPipeline);
    }

    public abstract void Dispose();
}
