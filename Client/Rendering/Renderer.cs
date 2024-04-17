using System;
using System.Collections.Generic;
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

    public abstract void CreatePipeline(MainFramebuffer framebuffer);

    public abstract void Render(double delta);

    public abstract void Dispose();
}


public abstract class RendererDependency {
    public readonly List<RendererDependency> Dependencies = [];

    public RendererDependency? parent { get; private set; } = null;

    internal RendererDependency() {}

    /// <summary>
    /// Defines a dependency for this renderer.
    /// <br/>
    /// The dependency's render call will be made after this one's
    /// </summary>
    /// <exception cref="ArgumentException">
    /// If the target renderer already has a parent
    /// </exception>
    public void DependsOn(params RendererDependency[] dependencies) {
        foreach (var dependency in dependencies) {
            if (dependency.parent != null)
                throw new ArgumentException($"Renderer {dependency.GetType().Name} has a parent.");
            Dependencies.Add(dependency);
            dependency.parent = this;
        }
    }

    public virtual void Reload(MainFramebuffer buffer) {
        // Update children
        foreach (var d in Dependencies)
            d.Reload(buffer);
    }

    public virtual void PreRender(double delta) {}

    public virtual void PostRender(double delta) {}
}

public class ReloadableDependency<T> : RendererDependency {

}

public abstract class NewRenderer : RendererDependency, IDisposable {
    public readonly VoxelClient Client;
    public readonly RenderSystem RenderSystem;
    public readonly ResourceFactory ResourceFactory;

    public readonly CommandList CommandList;

    public readonly RenderPhase Phase;

    private readonly List<(uint, ResourceSet)> ResourceSets = [];

    private Pipeline? pipeline = null;

    public NewRenderer(VoxelClient client, RenderPhase phase = RenderPhase.PostRender) {
        Client = client;
        RenderSystem = client.RenderSystem;
        ResourceFactory = RenderSystem.ResourceFactory;
        CommandList = RenderSystem.MainCommandList;
        Phase = phase;
    }

    /// <summary>
    /// Defines a resource set to be applied before rendering
    /// </summary>
    public void WithResourceSet(uint idx, ResourceSet set)
        => ResourceSets.Add((idx, set));

    public override void PreRender(double delta) {
        if (Phase == RenderPhase.PreRender)
            RenderDependencies(delta);
    }

    public override void PostRender(double delta) {
        if (Phase == RenderPhase.PostRender)
            RenderDependencies(delta);
    }

    public void RenderDependencies(double delta) {
        foreach (var d in Dependencies)
            d.PreRender(delta);

        // Set the pipeline if it exists
        if (pipeline != null)
            CommandList.SetPipeline(pipeline);

        // Set the resource sets
        foreach (var (idx, set) in ResourceSets)
            CommandList.SetGraphicsResourceSet(idx, set);
        Render(delta);

        foreach (var d in Dependencies)
            d.PostRender(delta);
    }

    public override void Reload(MainFramebuffer buffer) {
        pipeline = CreatePipeline(buffer);
        base.Reload(buffer);
    }

    public virtual Pipeline? CreatePipeline(MainFramebuffer buffer)
        => null;
    
    public virtual void Render(double delta) {}
    
    public abstract void Dispose();

    public enum RenderPhase {
        PreRender,
        PostRender
    }
}
