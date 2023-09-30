using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Voxel.Client.Rendering;
using Voxel.Common.World;

namespace Voxel.Client.World;

public class ClientWorld {
    public Common.World.World world;

    public GraphicsDevice graphicsDevice;

    public ConcurrentDictionary<ChunkPos, ChunkMesh> loadedChunks = new();

    public ConcurrentQueue<ChunkPos> loadQueue = new();
    public ConcurrentQueue<ChunkPos> buildQueue = new();

    public ConcurrentQueue<ChunkPos> unloadQueue = new();

    public ClientWorld(Common.World.World world, GraphicsDevice graphicsDevice) {
        this.world = world;
        this.graphicsDevice = graphicsDevice;

        world.OnChunkLoaded += cs => {
            var chunks = cs.OrderBy(it => VoxelClient.Instance!.camera!.DistanceTo(it.ToVector()));
            foreach (var c in chunks) {
                OnChunkLoaded(c);
            }
        };
        world.OnChunkUnloaded += cs => {
            foreach (var c in cs) {
                OnChunkUnloaded(c);
            }
        };
        world.OnChunkChanged += cs => {
            foreach (var c in cs) {
                OnChunkChanged(c);
            }
        };
    }

    private void OnChunkLoaded(ChunkPos pos) {
        AddToLoadQueue(pos);
        AddToRebuildQueue(pos.Up());
        AddToRebuildQueue(pos.Down());
        AddToRebuildQueue(pos.North());
        AddToRebuildQueue(pos.South());
        AddToRebuildQueue(pos.East());
        AddToRebuildQueue(pos.West());
    }
    
    private void OnChunkChanged(ChunkPos pos) {
        AddToRebuildQueue(pos);
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

    private void AddToLoadQueue(ChunkPos pos) {
        if (
            !loadQueue.Contains(pos) &&
            !loadedChunks.ContainsKey(pos)
        ) loadQueue.Enqueue(pos);
    }
    
    private void AddToRebuildQueue(ChunkPos pos) {
        if (
            !buildQueue.Contains(pos) &&
            loadedChunks.ContainsKey(pos)
        ) buildQueue.Enqueue(pos);
    }

    private void AddToUnloadQueue(ChunkPos pos) {
        if (
            !unloadQueue.Contains(pos) &&
            loadedChunks.ContainsKey(pos)
        )
            buildQueue.Enqueue(pos);
    }

    public void LoadChunks() {
        while (loadQueue.TryDequeue(out var pos)) {
            if (unloadQueue.Contains(pos) || buildQueue.Contains(pos))
                continue;
            loadedChunks.TryGetValue(pos, out ChunkMesh? chunk);
            if (chunk != null) 
                continue;
            chunk = new();
            Monitor.Enter(loadedChunks);
            loadedChunks[pos] = chunk;
            Monitor.Exit(loadedChunks);
            AddToRebuildQueue(pos);
        }
    }

    public void BuildChunks(int threadNumber) {
        while (buildQueue.TryDequeue(out var pos)) {
            if (unloadQueue.Contains(pos))
                continue;
            loadedChunks.TryGetValue(pos, out ChunkMesh? chunk);
            chunk?.BuildChunk(graphicsDevice, this, pos, threadNumber);
        }
    }

    public void UnloadChunks() {
        while (unloadQueue.TryDequeue(out var pos)) {
            if (!loadedChunks.ContainsKey(pos))
                continue;
            Monitor.Enter(loadedChunks);
            loadedChunks.Remove(pos, out _);
            Monitor.Exit(loadedChunks);
        }
    }

    public void Draw(Effect effect, Camera camera) {
        if (!Monitor.TryEnter(loadedChunks, 5))
            return;
        var chunks = loadedChunks.OrderBy(it => camera.DistanceTo(it.Key.ToVector())).ToArray();
        Monitor.Exit(loadedChunks);

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
                (new BoundingFrustum(camera.View).Contains(new BoundingBox(pos, pos + new System.Numerics.Vector3(32, 32, 32))) != ContainmentType.Disjoint)
            )
                chunk.Draw(graphicsDevice, effect);
        }
    }
}
