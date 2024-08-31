using GlmSharp;
using Foxel.Common.Util;
using Foxel.Common.World.Views;

namespace Foxel.Common.World;

public class LoadedChunkSection {
    public readonly VoxelWorld World;

    public ivec3 centerPos { get; private set; }
    public int halfWidth { get; private set; }
    public int halfHeight { get; private set; }

    private ivec3 min => new(-halfWidth, -halfHeight, -halfWidth);
    private ivec3 max => new(halfWidth + 1, halfHeight + 1, halfWidth + 1);

    private Dictionary<ivec3, ChunkView> views = new();


    public event Action<Chunk> OnChunkAddedToView = _ => {};
    public event Action<Chunk> OnChunkRemovedFromView = _ => {};

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

    public IEnumerable<Chunk> Chunks() {
        foreach (var view in views.Values)
            yield return view.Chunk;
    }

    public bool ContainsPosition(dvec3 worldPosition) {
        var chunkPos = worldPosition.WorldToChunkPosition();
        return views.ContainsKey(chunkPos - centerPos);
    }

    private void Update() {
        var map = new Dictionary<ivec3, ChunkView>();
        foreach (var pos in Iteration.Cubic(min, max)) {

            bool hadAlready = views.ContainsKey(centerPos + pos);
            var view = World.GetOrCreateChunkView(centerPos + pos);
            map[pos] = view;

            if (!hadAlready)
                OnChunkAddedToView(view.Chunk);
        }

        foreach (var (key, value) in views) {
            //If it's not in the new map, it was un-viewed.
            if (!map.ContainsKey(key))
                OnChunkRemovedFromView(value.Chunk);

            value.Dispose();
        }

        views = map;
    }
}
