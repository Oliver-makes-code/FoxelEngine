using System.Runtime.InteropServices;

namespace Voxel.Common.World.Component;

public class ComponentHolder<TComponentType> {
    private readonly byte[] ComponentData;
    internal readonly List<Type> Components = [];
    internal readonly List<int> Sizes = [];

    public ComponentHolder(ComponentBuilder<TComponentType> builder) {
        foreach (byte b in builder.componentData)
            Console.WriteLine(b);
        ComponentData = builder.componentData;
        Components = builder.Components;
        Sizes = builder.Sizes;
    }

    public TComponent GetComponent<TComponent>() where TComponent : struct, TComponentType {
        var type = typeof(TComponent);
        int idx = 0;

        for (int i = 0; i < Components.Count; i++) {
            if (Components[i] == type) {
                // SAFETY: This is within bounds
                unsafe {
                    fixed (byte *arr = &ComponentData[0]) {
                        return *(TComponent*)(arr+idx);
                    }
                }
            }
            idx += Sizes[i];
        }

        return default;
    }
}

public class ComponentBuilder<TComponentType> {
    internal readonly List<Type> Components = [];
    internal readonly List<int> Sizes = [];
    internal byte[] componentData = [];

    // TODO: Figure out how to handle strings and maybe other reference types?
    public void Add<TComponent>(TComponent? defaultValue = null) where TComponent : struct, TComponentType {
        if (Components.Contains(typeof(TComponent)))
            return;
        Components.Add(typeof(TComponent));
        int size = Marshal.SizeOf<TComponent>();
        if (size % 4 != 0)
            size += 4 - (size % 4);
        int oldSize = componentData.Length;
        Array.Resize(ref componentData, oldSize + size);

        // SAFETY: We know it'll fit the new data.
        unsafe {
            fixed (byte *arr = &componentData[0]) {
                *(TComponent*)(arr + oldSize) = defaultValue ?? new();
            }
        }

        Sizes.Add(size);
    }
}
