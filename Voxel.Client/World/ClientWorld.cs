using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Voxel.Client.Rendering;
using Voxel.Common.World;

namespace Voxel.Client.World;

public class ClientWorld {
    public Common.World.World world;

    public GraphicsDevice graphicsDevice;

    public Dictionary<ChunkPos, ChunkMesh> loadedChunks = new();

    public List<ChunkPos> buildQueue = new();

    public List<ChunkPos> unloadQueue = new();

    public ClientWorld(Common.World.World world, GraphicsDevice graphicsDevice) {
        this.world = world;
        this.graphicsDevice = graphicsDevice;

        world.OnChunkLoaded += OnChunkLoaded;
        world.OnChunkUnloaded += OnChunkUnloaded;
    }

    private void OnChunkLoaded(ChunkPos pos) {
        AddToBuildQueue(pos);
        AddToRebuildQueue(pos.Up());
        AddToRebuildQueue(pos.Down());
        AddToRebuildQueue(pos.North());
        AddToRebuildQueue(pos.South());
        AddToRebuildQueue(pos.East());
        AddToRebuildQueue(pos.West());
    }

    private void OnChunkUnloaded(ChunkPos pos) {
        AddToUnloadQueue(pos);
        AddToRebuildQueue(pos.Up());
        AddToRebuildQueue(pos.Down());
        AddToRebuildQueue(pos.North());
        AddToRebuildQueue(pos.South());
        AddToRebuildQueue(pos.East());
        AddToRebuildQueue(pos.West());
    }

    private void AddToBuildQueue(ChunkPos pos) {
        if (!buildQueue.Contains(pos))
            buildQueue.Add(pos);
    }
    
    private void AddToRebuildQueue(ChunkPos pos) {
        if (loadedChunks.ContainsKey(pos))
            AddToBuildQueue(pos);
    }

    private void AddToUnloadQueue(ChunkPos pos) {
        if (!unloadQueue.Contains(pos))
            unloadQueue.Add(pos);
    }

    public void BuildOneChunk() {
        if (buildQueue.Count == 0)
            return;
        var pos = buildQueue[0];
        loadedChunks.TryGetValue(pos, out ChunkMesh? chunk);
        if (chunk == null) {
            loadedChunks[pos] = new(graphicsDevice, this, pos);
        } else {
            chunk.BuildChunk(graphicsDevice, this, pos);
        }
        buildQueue.RemoveAt(0);
    }

    public void UnloadChunks() {
        foreach (var pos in unloadQueue) {
            loadedChunks.TryGetValue(pos, out ChunkMesh? chunk);
            if (chunk == null)
                continue;
            loadedChunks.Remove(pos);
        }
    }

    public void Draw(Effect effect, Camera camera) {
        var chunks = loadedChunks.OrderBy(it => camera.DistanceTo(it.Key.ToVector()));

        foreach (var pair in chunks) {
            var pos = pair.Key.ToVector();
            var chunk = pair.Value;

            if (
                camera.IsPointVisible(pos) ||
                camera.IsPointVisible(pos + new Vector3(0, 0, 32)) ||
                camera.IsPointVisible(pos + new Vector3(0, 32, 0)) ||
                camera.IsPointVisible(pos + new Vector3(0, 32, 32)) ||
                camera.IsPointVisible(pos + new Vector3(32, 0, 0)) ||
                camera.IsPointVisible(pos + new Vector3(32, 0, 32)) ||
                camera.IsPointVisible(pos + new Vector3(32, 32, 0)) ||
                camera.IsPointVisible(pos + new Vector3(32, 32, 32)) ||
                new BoundingFrustum(camera.View).Contains(new BoundingBox(pos, pos + new System.Numerics.Vector3(32, 32, 32))) == ContainmentType.Intersects
            )
                chunk.Draw(graphicsDevice, effect);
        }
    }
}
