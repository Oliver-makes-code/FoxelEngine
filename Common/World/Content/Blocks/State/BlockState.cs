using System.Diagnostics.CodeAnalysis;
using Foxel.Common.Collections;
using Foxel.Common.Util;
using Foxel.Core.Util;
using Greenhouse.Libs.Serialization;
using Greenhouse.Libs.Serialization.Reader;
using Greenhouse.Libs.Serialization.Structure;

namespace Foxel.Common.World.Content.Blocks.State;

public readonly struct BlockState {
    public static readonly Codec<BlockState> Codec = new FoxelPrimitiveImplCodec<BlockState>(
        (reader) => {
            using var obj = reader.Object();
            var blockKey = ResourceKey.Codec.ReadGeneric(obj.Field("block"));
            var block = ContentStores.Blocks.GetValue(blockKey);
            var state = block.DefaultState;

            using var map = obj.Field("state").Map();
            for (int i = 0; i < map.Length(); i++) {
                var field = map.Field(out var name);
                var prop = block.Map.GetByName(name);
                var value = prop.ValueCodec().Read(field);
                state = state.WithObject(prop, value!);
            }

            return state;
        },
        (writer, value) => {
            using var obj = writer.Object(2);
            var block = value.Block;
            var blockKey = block.key;
            ResourceKey.Codec.WriteGeneric(obj.Field("block"), blockKey);

            int len = block.Map.Map.Length;
            using var map = obj.Field("state").Map(len);

            foreach (var (prop, _) in block.Map.Map) {
                var field = value.GetObject(prop);
                prop.ValueCodec().Write(map.Field(prop.GetName()), field);
            }
        }
    );

    public readonly Block Block;
    public readonly uint RawState = 0;
    public readonly BlockSettings Settings => Block.Settings;

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

    public object GetObject(BlockProperty property) {
        if (!Block.Map.Get(property, RawState, out uint value))
            throw new($"Property {property.GetName()} does not exist on block {Block}.");
        if (!property.ValidIndex((byte)value))
            throw new($"Block state index {value} out of range for property {property.GetName()}");
        return property.GetValueObject((byte)value);
    }

    public BlockState With<TValue>(BlockProperty<TValue> property, TValue value) where TValue : struct {
        if (!property.ValidValue(value))
            throw new($"Block state value {value} out of range for property {property.GetName()}");
        if (!Block.Map.Set(property, RawState, property.GetIndex(value), out uint rawState))
            throw new($"Property {property.GetName()} does not exist on block {Block}.");
        return new(Block, rawState);
    }

    public BlockState WithObject(BlockProperty property, object value) {
        if (!property.ValidValueObject(value))
            throw new($"Block state value {value} out of range for property {property.GetName()}");
        if (!Block.Map.Set(property, RawState, property.GetIndexObject(value), out uint rawState))
            throw new($"Property {property.GetName()} does not exist on block {Block}.");
        return new(Block, rawState);
    }

    public static bool operator == (BlockState lhs, BlockState rhs)
        => lhs.Block == rhs.Block && lhs.RawState == rhs.RawState;

    public static bool operator != (BlockState lhs, BlockState rhs)
        => lhs.Block != rhs.Block || lhs.RawState != rhs.RawState;

    public override bool Equals(object? obj)
        => obj is BlockState state && state == this;

    public override int GetHashCode()
        =>  Block.GetHashCode() << 17 | RawState.GetHashCode();
}

public readonly struct PartialBlockState(ResourceKey blockKey, Dictionary<string, StructuredValue> state) {
    public static readonly Codec<Dictionary<string, StructuredValue>> MapCodec = new FoxelPrimitiveImplCodec<Dictionary<string, StructuredValue>>(
        (reader) => {
            Dictionary<string, StructuredValue> values = [];
            using var map = reader.Map();
            for (int i = 0; i < map.Length(); i++) {
                var value = map.Field(out var key);
                values[key] = ((StructuredObjectDataReader)value).Value;
            }
            return values;
        },
        (writer, value) => {}
    );

    public static readonly Codec<PartialBlockState> Codec = RecordCodec<PartialBlockState>.Create(
        ResourceKey.Codec.Field<PartialBlockState>("block", it => it.Block.Key),
        MapCodec.DefaultedField<PartialBlockState>("state", it => it.State, () => []),
        (key, state) => new(key, state)
    );

    public readonly ContentReference<Block> Block = new(ContentStores.Blocks, blockKey);
    public readonly Dictionary<string, StructuredValue> State = state;

    private static BlockState Apply(Block block, Dictionary<string, StructuredValue> partial) {
        var state = block.DefaultState;
        foreach (var key in partial.Keys) {
            var prop = block.Map.GetByName(key);
            var value = prop.ValueCodec().Read(new StructuredObjectDataReader(partial[key]));
            state = state.WithObject(prop, value!);
        }
        return state;
    }

    public BlockState Get()
        => Apply(Block.Get(), State);
}

public readonly struct BlockStateMap {
    public readonly (BlockProperty, BitSpan)[] Map;

    private BlockStateMap((BlockProperty, BitSpan)[] map) {
        Map = map;
    }

    public BlockProperty GetByName(string name)
        => Map.Where(it => it.Item1.GetName() == name).First().Item1;

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
