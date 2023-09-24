using System.Collections.Concurrent;
using System.Collections.Generic;
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

    public List<ChunkPos> buildQueue = new();
    public List<ChunkPos> rebuildQueue = new();

    public List<ChunkPos> unloadQueue = new();

    public ClientWorld(Common.World.World world, GraphicsDevice graphicsDevice) {
        this.world = world;
        this.graphicsDevice = graphicsDevice;

        world.OnChunkLoaded += cs => {
            foreach (var c in cs) {
                OnChunkLoaded(c);
            }
        };
        world.OnChunkUnloaded += cs => {
            foreach (var c in cs) {
                OnChunkUnloaded(c);
            }
        };
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
        if (
            !buildQueue.Contains(pos) &&
            !loadedChunks.ContainsKey(pos)
        ) {
            Monitor.Enter(buildQueue);
            buildQueue.Add(pos);
            Monitor.Exit(buildQueue);
        }
    }
    
    private void AddToRebuildQueue(ChunkPos pos) {
        if (
            !rebuildQueue.Contains(pos) &&
            loadedChunks.ContainsKey(pos)
        ) {
            Monitor.Enter(rebuildQueue);
            rebuildQueue.Add(pos);
            Monitor.Exit(rebuildQueue);
        }
    }

    private void AddToUnloadQueue(ChunkPos pos) {
        if (!unloadQueue.Contains(pos)) {
            Monitor.Enter(unloadQueue);
            unloadQueue.Add(pos);
            Monitor.Exit(unloadQueue);
        }
    }

    public void BuildChunks() {
        while (buildQueue.Count != 0) {
            var pos = buildQueue[0];

            loadedChunks.TryGetValue(pos, out ChunkMesh? chunk);
            if (chunk == null) {
                chunk = new(graphicsDevice, this, pos);
                Monitor.Enter(loadedChunks);
                loadedChunks[pos] = chunk;
                Monitor.Exit(loadedChunks);
            }

            Monitor.Enter(buildQueue);
            buildQueue.RemoveAt(0);
            Monitor.Exit(buildQueue);
        }
    }

    public void RebuildChunks() {
        while (rebuildQueue.Count != 0) {
            var pos = rebuildQueue[0];

            loadedChunks.TryGetValue(pos, out ChunkMesh? chunk);
            chunk?.BuildChunk(graphicsDevice, this, pos);

            Monitor.Enter(rebuildQueue);
            rebuildQueue.RemoveAt(0);
            Monitor.Exit(rebuildQueue);
        }
    }

    public void UnloadChunks() {
        while (unloadQueue.Count != 0) {
            var pos = unloadQueue[0];
            if (loadedChunks.ContainsKey(pos)) {
                Monitor.Enter(loadedChunks);
                loadedChunks.Remove(pos, out _);
                Monitor.Exit(loadedChunks);
            }
            Monitor.Enter(unloadQueue);
            unloadQueue.RemoveAt(0);
            Monitor.Exit(unloadQueue);
        }
    }

    public void Draw(Effect effect, Camera camera, out List<(Vector2, string)> points) {
        points = new();

        if (!Monitor.TryEnter(loadedChunks, 0))
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
                chunk.Draw(graphicsDevice, effect, pos, camera, points);
        }
    }
}
