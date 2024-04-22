using System;
using System.Collections.Generic;
using Veldrid;
using Voxel.Core;
using Voxel.Core.Assets;
using Voxel.Core.Rendering;

namespace Voxel.Client.Rendering;

public class RendererDependency : IDisposable {
    public virtual void Reload(PackManager packs, RenderSystem renderSystem, MainFramebuffer buffer) {}

    public virtual void PreRender(double delta) {}

    public virtual void PostRender(double delta) {}

    public virtual void Dispose() {}
}

public class ReloadableDependency<T> : RendererDependency {
    public delegate T ValueCreator(PackManager packs, RenderSystem renderSystem, MainFramebuffer buffer);

    public readonly ValueCreator Creator;

    public T? value { get; private set; }

    public ReloadableDependency(ValueCreator creator) {
        Creator = creator;
    }

    public override void Reload(PackManager packs, RenderSystem renderSystem, MainFramebuffer buffer) {
        try {
            value = Creator(packs, renderSystem, buffer);
        } catch (Exception e) {
            Game.Logger.Error(e);
        }
    }
}

public abstract class Renderer : RendererDependency, IDisposable {
    public readonly List<RendererDependency> Dependencies = [];
    public readonly VoxelClient Client;
    public readonly RenderSystem RenderSystem;
    public readonly ResourceFactory ResourceFactory;

    public readonly CommandList CommandList;

    public readonly RenderPhase Phase;

    private readonly List<(uint, Func<ResourceSet>)> ResourceSets = [];

    public RendererDependency? parent { get; private set; } = null;

    protected Pipeline? pipeline = null;

    public Renderer(VoxelClient client, RenderPhase phase = RenderPhase.PostRender) {
        Client = client;
        RenderSystem = client.renderSystem!;
        ResourceFactory = RenderSystem.ResourceFactory;
        CommandList = RenderSystem.MainCommandList;
        Phase = phase;
    }

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
            Dependencies.Add(dependency);
            if (dependency is Renderer renderer) {
                if (renderer.parent != null)
                    throw new ArgumentException($"Renderer {dependency.GetType().Name} has a parent.");
                renderer.parent = this;
            }
        }
    }

    /// <summary>
    /// Defines a resource set to be applied before rendering
    /// </summary>
    public void WithResourceSet(uint idx, Func<ResourceSet> set)
        => ResourceSets.Add((idx, set));
    
    public void RecreatePipelines(MainFramebuffer framebuffer) {
        foreach (var d in Dependencies) {
            if (d is Renderer renderer) {
                renderer.RecreatePipelines(framebuffer);
            }
        }
        pipeline = CreatePipeline(Client.PackManager, framebuffer);
    }

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
            CommandList.SetGraphicsResourceSet(idx, set());
        Render(delta);

        foreach (var d in Dependencies)
            d.PostRender(delta);
    }

    public override void Reload(PackManager packs, RenderSystem renderSystem, MainFramebuffer buffer) {
        try {
            // Update children
            foreach (var d in Dependencies)
                d.Reload(packs, renderSystem, buffer);
            pipeline = CreatePipeline(packs, buffer);
        } catch (Exception e) {
            Game.Logger.Error(e);
        }
    }

    public virtual Pipeline? CreatePipeline(PackManager packs, MainFramebuffer buffer)
        => null;
    
    public virtual void Render(double delta) {}
    
    public override void Dispose() {
        foreach (var d in Dependencies)
            d.Dispose();
    }

    public enum RenderPhase {
        PreRender,
        PostRender
    }
}
