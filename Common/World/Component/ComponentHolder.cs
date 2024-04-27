using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Voxel.Common.World.Component;

public class ComponentHolder<TComponentType> {
    private readonly byte[] ComponentData;
    internal readonly Type[] Components = [];
    internal readonly Ref<TComponentType>[] References;
    internal readonly int[] Sizes = [];

    public ComponentHolder(ComponentBuilder<TComponentType> builder) {
        ComponentData = builder.componentData;
        Components = [..builder.Components];
        Sizes = [..builder.Sizes];
        References = [..builder.References];
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
                reference.UpdateFunc((byte *)ptr);
                return true;
            }
        }
        return false;
    }

    private unsafe bool GetComponentPointer<TComponent>([NotNullWhen(true)] out TComponent *ptr, [NotNullWhen(true)] out Ref<TComponentType>? reference)  where TComponent : struct, TComponentType {
        var type = typeof(TComponent);
        int idx = 0;

        for (int i = 0; i < Components.Length; i++) {
            if (Components[i] == type) {
                fixed (byte *arr = &ComponentData[0]) {
                    ptr = (TComponent*)(arr+idx);
                    reference = References[i];
                    return true;
                }
            }
            idx += Sizes[i];
        }

        ptr = (TComponent*)0;
        reference = null;
        return false;
    }
}

public class ComponentBuilder<TComponentType> {
    internal readonly List<Type> Components = [];
    internal readonly List<int> Sizes = [];
    internal readonly List<Ref<TComponentType>> References = [];
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
                    References[i].UpdateFunc(arr+idx);
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
                var componentRef = new Ref<TComponentType>(*component, ptr => *(TComponent*)ptr);
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
/// <typeparam name="TComponentType"></typeparam>
internal class Ref<TComponentType> {
    public unsafe delegate TComponentType Updater(byte *ptr);
    public readonly Updater UpdateFunc;
    public TComponentType value;

    public Ref(TComponentType value, Updater updater) {
        UpdateFunc = updater;
        this.value = value;
    }
}
