using GlmSharp;
using Foxel.Common.Util;
using Greenhouse.Libs.Serialization;
using Foxel.Common.World.Content.Blocks.State;

namespace Foxel.Common.World.Storage;

public abstract class ChunkStorage : IDisposable {
    public static readonly Codec<StorageType> TypeCodec = new IntEnumCodec<StorageType, byte>(Codecs.Byte);
    public static readonly Codec<ChunkStorage> Codec = new ProxyCodec<Variant<StorageType, ChunkStorage>, ChunkStorage>(
        new VariantCodec<StorageType, ChunkStorage>(
            TypeCodec,
            (it) => it switch {
                StorageType.Single => SingleStorage.Codec,
                StorageType.Simple => SimpleStorage.Codec,
                StorageType.Void => VoidStorage.Codec,
                _ => throw new ArgumentException()
            }
        ),
        (variant) => variant.value,
        (value) => new(
            value switch {
                SingleStorage => StorageType.Single,
                SimpleStorage => StorageType.Simple,
                VoidStorage => StorageType.Void,
                _ => throw new ArgumentException()
            },
            value
        )
    );

    /// <summary>
    /// Accesses a block at a given 
    /// </summary>
    /// <param name="index"></param>
    public BlockState this[int index] {
        get => GetBlock(index);
        set => SetBlock(value, index);
    }

    public BlockState this[uint index] {
        get => this[(int) index];
        set => this[(int)index] = value;
    }

    /// <summary>
    /// Gets a block at a given local position.
    /// </summary>
    /// <param name="position"></param>
    public BlockState this[ivec3 position] {
        get => this[position.ToBlockIndex()];
        set => this[position.ToBlockIndex()] = value;
    }

    public abstract ChunkStorage WithChunk(Chunk chunk);

    public abstract ChunkStorage GenerateCopy();

    public abstract void Dispose();

    public abstract Codec<ChunkStorage> GetCodec();

    internal abstract void SetBlock(BlockState toSet, int index);
    internal abstract BlockState GetBlock(int index);
}

public enum StorageType : byte {
    Single,
    Simple,
    Void
}
