using Voxel.Common.Util;

namespace Voxel.Common.World.Ecs;

public abstract class EcsEntity<TEntity, TBuilder>
where TEntity : EcsEntity<TEntity, TBuilder>
where TBuilder : EcsEntityBuilder<TEntity, TBuilder> {
    internal readonly Dictionary<Type, Delegate> EventListeners;

    public EcsEntity(TBuilder builder) {
        EventListeners = builder.EventListeners;
    }

    public void Invoke<TEvent>(Action<TEvent> invoker) where TEvent : Delegate {
        if (!EventListeners.TryGetValue(typeof(TEvent), out var ev))
            return;
        invoker((ev as TEvent)!);
    }
}

public abstract class EcsEntityBuilder<TEntity, TBuilder>
where TEntity : EcsEntity<TEntity, TBuilder>
where TBuilder : EcsEntityBuilder<TEntity, TBuilder> {
    internal readonly Dictionary<Type, Delegate> EventListeners = [];

    public EcsEntityBuilder() {}

    public void Listen<TEvent>(TEvent listener) where TEvent : Delegate {
        var type = typeof(TEvent);
        if (EventListeners.TryGetValue(type, out var ev))
            listener = Delegates.Combine(listener, (ev as TEvent)!);
        EventListeners[type] = listener;
    }
}

public abstract class ComponentEntity<TEntity, TInstance, TBuilder>(TBuilder builder) : EcsEntity<TEntity, TBuilder>(builder)
where TEntity : ComponentEntity<TEntity, TInstance, TBuilder>
where TInstance : ComponentEntityInstance<TEntity, TInstance, TBuilder>
where TBuilder : ComponentEntityBuilder<TEntity, TInstance, TBuilder> {
    public abstract TInstance NewInstance();
}

public abstract class ComponentEntityBuilder<TEntity, TInstance, TBuilder> : EcsEntityBuilder<TEntity, TBuilder>
where TEntity : ComponentEntity<TEntity, TInstance, TBuilder>
where TInstance : ComponentEntityInstance<TEntity, TInstance, TBuilder>
where TBuilder : ComponentEntityBuilder<TEntity, TInstance, TBuilder>;

public abstract class ComponentEntityInstance<TEntity, TInstance, TBuilder>
where TEntity : ComponentEntity<TEntity, TInstance, TBuilder>
where TInstance : ComponentEntityInstance<TEntity, TInstance, TBuilder>
where TBuilder : ComponentEntityBuilder<TEntity, TInstance, TBuilder> {
    public readonly TEntity Entity;

    public ComponentEntityInstance(TEntity entity) {
        Entity = entity;
    }

    public void Invoke<TEvent>(Action<TEvent> invoker) where TEvent : Delegate
        => Entity.Invoke(invoker);
}
