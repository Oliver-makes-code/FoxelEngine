using Foxel.Common.Collections;

namespace Foxel.Common.World.Content.Blocks.State;

public struct BlockState {
    public readonly NewBlock Block;
    private uint rawState = 0;

    public BlockState(NewBlock block) {
        Block = block;
    }

    public readonly TValue Get<TValue>(BlockProperty<TValue> property) where TValue : struct {
        if (!Block.Map.Get(property, rawState, out uint value))
            throw new($"Property {property.GetName()} does not exist on block {Block}.");
        if (!property.ValidIndex((byte)value))
            throw new($"Block state index {value} out of range for property {property.GetName()}");
        return property.GetValue((byte)value);
    }

    public void With<TValue>(BlockProperty<TValue> property, TValue value) where TValue : struct {
        if (!property.ValidValue(value))
            throw new($"Block state value {value} out of range for property {property.GetName()}");
        if (!Block.Map.Set(property, rawState, property.GetIndex(value), out rawState))
            throw new($"Property {property.GetName()} does not exist on block {Block}.");
    }
}

public readonly struct BlockStateMap {
    public readonly (BlockProperty, BitSpan)[] Map;

    private BlockStateMap((BlockProperty, BitSpan)[] map) {
        Map = map;
    }

    public bool Get(BlockProperty property, uint state, out uint value) {
        foreach (var (prop, span) in Map) {
            if (prop == property) {
                value = span.Get(state);
                return true;
            }
        }
        value = 0;
        return false;
    }

    public bool Set(BlockProperty property, uint state, uint value, out uint result) {
        foreach (var (prop, span) in Map) {
            if (prop == property) {
                result = span.Set(state, value);
                return true;
            }
        }
        result = state;
        return false;
    }

    public class Builder {
        private readonly List<(BlockProperty, BitSpan)> Map = [];
        private byte offset = 0;

        public void Add(BlockProperty property) {
            byte count = property.GetPropertyCount();
            byte length = 0;
            while (count > 0) {
                count >>= 1;
                length++;
            }
            var span = new BitSpan(offset, length);
            Map.Add((property, span));
            offset += length;
        }

        public BlockStateMap Build()
            => new([..Map]);
    }
}
