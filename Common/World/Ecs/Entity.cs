using Voxel.Common.Util;

namespace Voxel.Common.World.Ecs;

public abstract class EcsEntity<TSelf, TInstance> where TSelf : EcsEntity<TSelf, TInstance> where TInstance : EcsEntityInstance<TInstance, TSelf> {
    internal readonly Dictionary<Type, Delegate> EventListeners;

    public EcsEntity(EcsEntityBuilder<TSelf, TInstance> builder) {
        EventListeners = builder.EventListeners;
    }

    public abstract TInstance NewInstance();

    public void Invoke<TEvent>(Action<TEvent> invoker) where TEvent : Delegate {
        if (!EventListeners.TryGetValue(typeof(TEvent), out var ev))
            return;
        invoker((ev as TEvent)!);
    }
}

public class ComponentHoldingEcsEntity<TSelf> : EcsEntity<TSelf, ComponentHoldingEcsEntityInstance<TSelf>> where TSelf : ComponentHoldingEcsEntity<TSelf> {
    public ComponentHoldingEcsEntity(ComponentHoldingEcsEntityBuilder<TSelf> builder) : base(builder) {}

    public override ComponentHoldingEcsEntityInstance<TSelf> NewInstance()
        => new((TSelf)this);
}

public abstract class EcsEntityBuilder<TEntity, TInstance> where TEntity : EcsEntity<TEntity, TInstance> where TInstance : EcsEntityInstance<TInstance, TEntity> {
    internal readonly Dictionary<Type, Delegate> EventListeners = [];

    public EcsEntityBuilder() {}

    public void Listen<TEvent>(TEvent listener) where TEvent : Delegate {
        var type = typeof(TEvent);
        if (EventListeners.TryGetValue(type, out var ev))
            listener = Delegates.Combine(listener, (ev as TEvent)!);
        EventListeners[type] = listener;
    }
}

public sealed class ComponentHoldingEcsEntityBuilder<TEntity> : EcsEntityBuilder<TEntity, ComponentHoldingEcsEntityInstance<TEntity>> where TEntity : ComponentHoldingEcsEntity<TEntity> {
    public void Attach<TComponent>() where TComponent : struct, EcsComponent {
        // TODO
    }
}

public abstract class EcsEntityInstance<TSelf, TEntity> where TSelf : EcsEntityInstance<TSelf, TEntity> where TEntity : EcsEntity<TEntity, TSelf> {
    public readonly TEntity Entity;

    public EcsEntityInstance(TEntity entity) {
        Entity = entity;
    }

    public void Invoke<TEvent>(Action<TEvent> invoker) where TEvent : Delegate
        => Entity.Invoke(invoker);
}

public sealed class ComponentHoldingEcsEntityInstance<TEntity> : EcsEntityInstance<ComponentHoldingEcsEntityInstance<TEntity>, TEntity> where TEntity : ComponentHoldingEcsEntity<TEntity> {
    public ComponentHoldingEcsEntityInstance(TEntity entity) : base(entity) {}

    public void Get<TComponent>() where TComponent : struct, EcsComponent {
        // TODO
    }

    public void Set<TComponent>(TComponent component) where TComponent : struct, EcsComponent {
        // TODO
    }
}
