using GlmSharp;
using Voxel.Common.Util;
using Voxel.Common.World.Views;

namespace Voxel.Common.World;

public class LoadedChunkSection {
    private readonly VoxelWorld World;

    public ivec3 centerPos { get; private set; }
    public int halfWidth { get; private set; }
    public int halfHeight { get; private set; }

    private ivec3 min => new(-halfWidth, -halfHeight, -halfWidth);
    private ivec3 max => new(halfWidth + 1, halfHeight + 1, halfWidth + 1);

    private Dictionary<ivec3, ChunkView> views = new();

    public LoadedChunkSection(VoxelWorld world, ivec3 centerPos, int halfWidth, int halfHeight) {
        World = world;
        this.centerPos = centerPos;
        this.halfWidth = halfWidth;
        this.halfHeight = halfHeight;
        Update();
    }

    public Chunk? GetChunkRelative(ivec3 relativePos)
        => views.TryGetValue(relativePos, out var view) ? view.Chunk : null;

    public Chunk? GetChunkAbsolute(ivec3 absolutePos)
        => GetChunkRelative(absolutePos - centerPos);

    public void Move(ivec3 centerPos) {
        if (this.centerPos == centerPos)
            return;
        this.centerPos = centerPos;
        Update();
    }

    public void Resize(int halfWidth, int halfHeight) {
        if (this.halfWidth == halfWidth && this.halfHeight == halfHeight)
            return;
        this.halfWidth = halfWidth;
        this.halfHeight = halfHeight;
        Update();
    }

    private void Update() {
        var map = new Dictionary<ivec3, ChunkView>();
        foreach (var pos in Iteration.Cubic(min, max)) {
            map[pos] = World.GetOrCreateChunkView(centerPos + pos);
        }

        views = map;
    }
}