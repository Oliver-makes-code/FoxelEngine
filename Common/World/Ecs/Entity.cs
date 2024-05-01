using Voxel.Common.Util;

namespace Voxel.Common.World.Ecs;

public class EcsEntity<TSelf> where TSelf : EcsEntity<TSelf> {
    internal readonly Dictionary<Type, Delegate> EventListeners;

    public EcsEntity(EcsEntityBuilder<TSelf> builder) {
        EventListeners = builder.EventListeners;
    }

    public EcsEntityInstance<TSelf> NewInstance()
        => new((TSelf) this);

    public void Invoke<TEvent>(Action<TEvent> invoker) where TEvent : Delegate {
        if (!EventListeners.TryGetValue(typeof(TEvent), out var ev))
            return;
        invoker((ev as TEvent)!);
    }
}

public class EcsEntityBuilder<TEntity> where TEntity : EcsEntity<TEntity> {
    internal readonly Dictionary<Type, Delegate> EventListeners = [];

    public void Attach<TComponent>() where TComponent : struct, EcsComponent {
        // TODO
    }
    
    public void Listen<TEvent>(TEvent listener) where TEvent : Delegate {
        var type = typeof(TEvent);
        if (EventListeners.TryGetValue(type, out var ev))
            listener = Delegates.Combine(listener, (ev as TEvent)!);
        EventListeners[type] = listener;
    }
}

public class EcsEntityInstance<TEntity> where TEntity : EcsEntity<TEntity> {
    public readonly TEntity Entity;

    public EcsEntityInstance(TEntity entity) {
        Entity = entity;
    }

    public void Get<TComponent>() where TComponent : struct, EcsComponent {
        // TODO
    }

    public void Set<TComponent>(TComponent component) where TComponent : struct, EcsComponent {
        // TODO
    }

    public void Invoke<TEvent>(Action<TEvent> invoker) where TEvent : Delegate
        => Entity.Invoke(invoker);
}
