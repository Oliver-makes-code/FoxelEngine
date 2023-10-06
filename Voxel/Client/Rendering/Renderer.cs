using System;
using RenderSurface.Rendering;
using Veldrid;

namespace Voxel.Client.Rendering;

public abstract class Renderer : IDisposable {
    public readonly VoxelNewClient Client;
    public readonly RenderSystem RenderSystem;
    public readonly ResourceFactory ResourceFactory;

    public CommandList CommandList => RenderSystem.MainCommandList;

    public Renderer(VoxelNewClient client) {
        Client = client;
        RenderSystem = client.RenderSystem;
        ResourceFactory = RenderSystem.ResourceFactory;
    }

    public abstract void Render(double delta);

    public abstract void Dispose();
}
