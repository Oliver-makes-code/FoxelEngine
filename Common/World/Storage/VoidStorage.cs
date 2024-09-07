using Foxel.Common.World.Content.Blocks;
using Foxel.Common.World.Content.Blocks.State;
using Greenhouse.Libs.Serialization;

namespace Foxel.Common.World.Storage;

public class VoidStorage : ChunkStorage {
    public static new readonly Codec<ChunkStorage> Codec = new UnitCodec<ChunkStorage>(() => new VoidStorage());

    public override void Dispose() {}

    public override ChunkStorage GenerateCopy()
        => new VoidStorage();

    public override Codec<ChunkStorage> GetCodec()
        => Codec;
        
    public override ChunkStorage WithChunk(Chunk chunk)
        => new VoidStorage();
    
    internal override BlockState GetBlock(int index)
        => BlockStore.Blocks.Air.Get().DefaultState;

    internal override void SetBlock(BlockState toSet, int index) {}
}
