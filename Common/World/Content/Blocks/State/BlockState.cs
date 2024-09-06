using System.Diagnostics.CodeAnalysis;
using Foxel.Common.Collections;

namespace Foxel.Common.World.Content.Blocks.State;

public readonly struct BlockState {
    public readonly Block Block;
    public readonly uint RawState = 0;

    public BlockState(Block block) {
        Block = block;
    }

    public BlockState(Block block, uint rawState) {
        Block = block;
        RawState = rawState;
    }

    public static BlockState FromRawParts(int blockId, uint state) {
        var block = ContentStores.Blocks.GetValue(blockId);
        if (block.Map.IsValid(state, out var property, out byte value))
            return new(block, state);
        throw new ArgumentException($"Illegal state {value} for property {property.GetName()}");
    }

    public TValue Get<TValue>(BlockProperty<TValue> property) where TValue : struct {
        if (!Block.Map.Get(property, RawState, out uint value))
            throw new($"Property {property.GetName()} does not exist on block {Block}.");
        if (!property.ValidIndex((byte)value))
            throw new($"Block state index {value} out of range for property {property.GetName()}");
        return property.GetValue((byte)value);
    }

    public BlockState With<TValue>(BlockProperty<TValue> property, TValue value) where TValue : struct {
        if (!property.ValidValue(value))
            throw new($"Block state value {value} out of range for property {property.GetName()}");
        if (!Block.Map.Set(property, RawState, property.GetIndex(value), out uint rawState))
            throw new($"Property {property.GetName()} does not exist on block {Block}.");
        return new(Block, rawState);
    }

    public static bool operator == (BlockState lhs, BlockState rhs)
        => lhs.Block == rhs.Block && lhs.RawState == rhs.RawState;

    public static bool operator != (BlockState lhs, BlockState rhs)
        => lhs.Block != rhs.Block || lhs.RawState != rhs.RawState;
}

public readonly struct BlockStateMap {
    public readonly (BlockProperty, BitSpan)[] Map;

    private BlockStateMap((BlockProperty, BitSpan)[] map) {
        Map = map;
    }

    public bool IsValid(uint state, [NotNullWhen(false)] out BlockProperty? failedProperty, out byte failedValue) {
        foreach (var (prop, span) in Map) {
            byte value = (byte)span.Get(state);
            if (!prop.ValidIndex(value)) {
                failedProperty = prop;
                failedValue = value;
                return false;
            }
        }
        failedProperty = null;
        failedValue = 0;
        return true;
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
            if (prop.GetName() == property.GetName()) {
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

        public Builder Add(BlockProperty property) {
            foreach (var (prop, _) in Map)
                if (prop.GetName() == property.GetName())
                    throw new($"Duplicate proeprty {prop.GetName()}.");

            byte count = property.GetPropertyCount();
            byte length = 0;
            while (count > 0) {
                count >>= 1;
                length++;
            }
            var span = new BitSpan(offset, length);
            Map.Add((property, span));
            offset += length;
            return this;
        }

        public BlockStateMap Build()
            => new([..Map]);
    }
}
