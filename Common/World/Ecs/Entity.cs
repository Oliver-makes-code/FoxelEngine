using System.Linq;
using Voxel.Common.Util;
using Voxel.Core.Util;

namespace Voxel.Common.World.Ecs;

public abstract class EcsEntity<TEntity, TInstance, TBuilder>
where TEntity : EcsEntity<TEntity, TInstance, TBuilder>
where TInstance : EcsEntityInstance<TEntity, TInstance, TBuilder> 
where TBuilder : EcsEntityBuilder<TEntity, TInstance, TBuilder> {
    internal readonly Dictionary<ResourceKey, List<Delegate>> EventListeners;

    public EcsEntity(TBuilder builder) {
        EventListeners = builder.EventListeners;
    }

    public abstract TInstance NewInstance();

    public void Invoke(ResourceKey signal, params object?[]? args) {
        if (!EventListeners.TryGetValue(signal, out var listeners))
            return;
            
        foreach (var listener in listeners)
            listener.DynamicInvoke(args);
    }
}

public abstract class EcsEntityBuilder<TEntity, TInstance, TBuilder>
where TEntity : EcsEntity<TEntity, TInstance, TBuilder>
where TInstance : EcsEntityInstance<TEntity, TInstance, TBuilder> 
where TBuilder : EcsEntityBuilder<TEntity, TInstance, TBuilder> {
    internal readonly Dictionary<ResourceKey, List<Delegate>> EventListeners = [];

    public EcsEntityBuilder() {}

    public void Listen<TEvent>(ResourceKey signal, TEvent listener) where TEvent : Delegate {
        if (!EventListeners.ContainsKey(signal))
            EventListeners[signal] = [];
        EventListeners[signal].Add(listener);
    }
}

public abstract class EcsEntityInstance<TEntity, TInstance, TBuilder>
where TEntity : EcsEntity<TEntity, TInstance, TBuilder>
where TInstance : EcsEntityInstance<TEntity, TInstance, TBuilder> 
where TBuilder : EcsEntityBuilder<TEntity, TInstance, TBuilder> {
    public readonly TEntity Entity;

    public EcsEntityInstance(TEntity entity) {
        Entity = entity;
    }

    public void Invoke(ResourceKey signal, params object?[]? args)
        => Entity.Invoke(signal, args);
}

public abstract class ComponentEntity<TEntity, TInstance, TBuilder>(TBuilder builder) : EcsEntity<TEntity, TInstance, TBuilder>(builder)
where TEntity : ComponentEntity<TEntity, TInstance, TBuilder>
where TInstance : ComponentEntityInstance<TEntity, TInstance, TBuilder>
where TBuilder : ComponentEntityBuilder<TEntity, TInstance, TBuilder>;

public abstract class ComponentEntityBuilder<TEntity, TInstance, TBuilder> : EcsEntityBuilder<TEntity, TInstance, TBuilder>
where TEntity : ComponentEntity<TEntity, TInstance, TBuilder>
where TInstance : ComponentEntityInstance<TEntity, TInstance, TBuilder>
where TBuilder : ComponentEntityBuilder<TEntity, TInstance, TBuilder>;

public abstract class ComponentEntityInstance<TEntity, TInstance, TBuilder>(TEntity entity) : EcsEntityInstance<TEntity, TInstance, TBuilder>(entity)
where TEntity : ComponentEntity<TEntity, TInstance, TBuilder>
where TInstance : ComponentEntityInstance<TEntity, TInstance, TBuilder>
where TBuilder : ComponentEntityBuilder<TEntity, TInstance, TBuilder>;
