namespace Voxel.Common.World.Component;

public class ComponentHolder<TComponentType> {
    internal readonly TComponentType[] Components;

    public ComponentHolder(ComponentBuilder<TComponentType> builder) {
        Components = [..builder.Components];
    }

    public IEnumerable<TComponentType> ListComponents() {
        foreach (var component in Components)
            yield return component;
    }

    public bool GetComponent<TComponent>(TComponent value) where TComponent : struct, TComponentType {
        foreach (var component in Components) {
            if (component is TComponent c) {
                value = c;
                return true;
            }
        }
        return false;
    }

    public bool WriteComponent<TComponent>(TComponent value) where TComponent : struct, TComponentType {
        for (int i = 0; i < Components.Length; i++) {
            if (Components[i] is TComponent) {
                Components[i] = value;
                return true;
            }
        }
        return false;
    }
}

public class ComponentBuilder<TComponentType> {
    internal readonly HashSet<Type> ComponentTypes = [];
    internal readonly List<TComponentType> Components = [];

    public void Add<TComponent>() where TComponent : struct, TComponentType {
        if (ComponentTypes.Contains(typeof(TComponent)))
            return;
        ComponentTypes.Add(typeof(TComponent));
        Components.Add(new TComponent());
    }
}
