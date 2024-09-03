using Foxel.Core.Util;
using Greenhouse.Libs.Serialization;

namespace Foxel.Common.World.Content.Items.Components;

public class StackSizeItemComponent(uint maxStackSize) : ItemComponent {
    public static readonly ResourceKey Key = new("stack_size");

    public uint maxStackSize = maxStackSize;

    public RecordCodec<ItemComponent> GetCodec()
        => ItemStore.ComponentCodecs.StackSize;
}
