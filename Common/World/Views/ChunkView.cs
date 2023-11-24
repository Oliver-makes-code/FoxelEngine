namespace Voxel.Common.World.Views; 

public class ChunkView : IDisposable {
    public readonly Chunk Chunk;

    public ChunkView(Chunk chunk) {
        Chunk = chunk;
        chunk.IncrementViewCount();
    }
    
    public void Dispose() {
        Chunk.DecrementViewCount();
    }
}
