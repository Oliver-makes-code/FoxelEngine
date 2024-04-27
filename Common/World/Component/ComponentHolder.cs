using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Voxel.Common.World.Component;

public class ComponentHolder<TComponentType> {
    private readonly byte[] ComponentData;
    internal readonly Type[] Components = [];
    internal readonly ComponentRef<TComponentType>[] References;
    internal readonly int[] Sizes = [];

    public ComponentHolder(ComponentBuilder<TComponentType> builder) {
        ComponentData = builder.componentData;
        Components = [..builder.Components];
        Sizes = [..builder.Sizes];
        References = [..builder.References];
    }

    public IEnumerable<TComponentType> ListComponents() {
        foreach (var component in References)
            yield return component.Deref();
    }

    public bool GetComponent<TComponent>(out TComponent value) where TComponent : struct, TComponentType {
        // SAFETY: We know the pointer points to a valid address when true
        unsafe {
            if (GetComponentPointer<TComponent>(out var ptr, out _)) {
                value = *ptr;
                return true;
            }
        }
        value = default;
        return false;
    }

    public bool WriteComponent<TComponent>(TComponent value) where TComponent : struct, TComponentType {
        // SAFETY: We know the pointer points to a valid address when true
        unsafe {
            if (GetComponentPointer<TComponent>(out var ptr, out var reference)) {
                *ptr = value;
                return true;
            }
        }
        return false;
    }

    private unsafe bool GetComponentPointer<TComponent>([NotNullWhen(true)] out TComponent *ptr, [NotNullWhen(true)] out ComponentRef<TComponentType>? reference)  where TComponent : struct, TComponentType {
        var type = typeof(TComponent);
        int idx = 0;

        // Iterate through the components
        for (int i = 0; i < Components.Length; i++) {
            // If it's the same type, return the pointer
            if (Components[i] == type) {
                fixed (byte *arr = &ComponentData[0]) {
                    ptr = (TComponent*)(arr+idx);
                    reference = References[i];
                    return true;
                }
            }
            idx += Sizes[i];
        }

        // Return null
        ptr = (TComponent*)0;
        reference = null;
        return false;
    }
}

public class ComponentBuilder<TComponentType> {
    internal readonly List<Type> Components = [];
    internal readonly List<int> Sizes = [];
    internal readonly List<ComponentRef<TComponentType>> References = [];
    internal byte[] componentData = [];

    public void Add<TComponent>(TComponent? defaultValue = null) where TComponent : struct, TComponentType {
        if (Components.Contains(typeof(TComponent)))
            return;
        Components.Add(typeof(TComponent));
        // Calculate the size of the component
        int size = Marshal.SizeOf<TComponent>();
        if (size % 8 != 0)
            size += 8 - (size % 8);
        int oldSize = componentData.Length;

        // Create a new array with an expanded size
        byte[] newData = new byte[oldSize + size];

        // SAFETY: We know it's at least the size of the old array
        unsafe {
            fixed (byte *arr = &newData[0]) {
                int idx = 0;
                // Update the references to the new data array
                for (int i = 0; i < Sizes.Count; i++) {
                    References[i].Update(arr+idx);
                    idx += Sizes[i];
                }
            }
        }

        // Update the value
        componentData = newData;

        // SAFETY: We know it'll fit the new data.
        unsafe {
            fixed (byte *arr = &componentData[0]) {
                // Write the component
                var component = (TComponent*)(arr + oldSize);
                *component = defaultValue ?? new();

                // Add the reference to the list
                var componentRef = new ComponentRef<TComponentType, TComponent>(component);
                References.Add(componentRef);
            }
        }

        Sizes.Add(size);
    }
}

/// <summary>
/// A value storing a boxed reference to a component,
/// so the GC won't eat references in the components
/// </summary>
internal unsafe interface ComponentRef<TComponentType> {
    public void Update(byte *ptr);

    public TComponentType Deref();
}

/// <summary>
/// A value storing a boxed reference to a component,
/// so the GC won't eat references in the components
/// </summary>
internal unsafe class ComponentRef<TComponentType, TComponent> : ComponentRef<TComponentType> where TComponent : struct, TComponentType {
    public TComponent *value;

    public ComponentRef(TComponent *value) {
        this.value = value;
    }

    public void Update(byte *ptr)
        => value = (TComponent *)ptr;

    public TComponentType Deref()
        => *value;
}
