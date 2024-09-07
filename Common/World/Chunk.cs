using GlmSharp;
using Foxel.Common.Util;
using Foxel.Common.World.Storage;
using Foxel.Common.World.Tick;
using Foxel.Common.World.Content.Entities;
using Foxel.Common.World.Content.Blocks;
using Foxel.Common.World.Content.Blocks.State;

namespace Foxel.Common.World;

public class Chunk : Tickable, IDisposable {
    public const int RandomTickCount = 32;
    
    /// The position of the chunk in chunk-space, one unit is one chunk
    public readonly ivec3 ChunkPosition;
    /// The position of the chunk in world-space, one unit is one block
    public readonly ivec3 WorldPosition;

    public readonly VoxelWorld World;

    public readonly TickList TickList = new();
    public readonly List<Entity> Entities = new();

    public ChunkStorage storage { get; private set; }

    public bool isEmpty => storage is VoidStorage || (storage is SingleStorage ss && ss.State == BlockStore.Blocks.Air.Get().DefaultState);

    private uint viewCount;

    /// <summary>
    /// When a chunk is modified, this is incremented. This is used to tell systems like networking or chunk meshing that the chunk has changed.
    /// </summary>
    private uint _version;

    public Chunk(ivec3 chunkPosition, VoxelWorld world, ChunkStorage? storage = null) {
        if (chunkPosition.y > world.GetMaxChunkHeight() || chunkPosition.y < world.GetMinChunkHeight())
            this.storage = new VoidStorage();
        else
            this.storage = storage ?? new SingleStorage(BlockStore.Blocks.Air.Get().DefaultState, this);

        ChunkPosition = chunkPosition;
        WorldPosition = ChunkPosition * PositionExtensions.ChunkSize;
        World = world;
    }

    public ChunkStorage CopyStorage()
        => storage.GenerateCopy();

    public virtual void SetStorage(ChunkStorage newStorage) {
        if (ChunkPosition.y > World.GetMaxChunkHeight() || ChunkPosition.y < World.GetMinChunkHeight())
            return;
        storage.Dispose();
        storage = newStorage;
        IncrementVersion();
    }

    public virtual void SetBlockState(ivec3 position, BlockState toSet) {
        storage[position] = toSet;
        IncrementVersion();
    }
    
    public BlockState GetBlockState(ivec3 position)
        => storage[position];

    public uint GetVersion()
        => _version;

    public void IncrementVersion() {
        Interlocked.Increment(ref _version);
    }
    
    // I don't know if this is the best place to put this..
    public HashSet<ivec3> FloodFill(ivec3 root) {
        var connected = new HashSet<ivec3>();

        if (storage is VoidStorage) {
            connected.UnionWith(Iteration.Cubic(PositionExtensions.ChunkSize));
            return connected;
        }

        if (storage is SingleStorage singleStorage) {
            if (singleStorage.State.Settings.IsNonSolid)
                connected.UnionWith(Iteration.Cubic(PositionExtensions.ChunkSize));
            return connected;
        }
        
        var queue = new Queue<ivec3>();
        queue.Add(root.Loop(PositionExtensions.ChunkSize));
        while (queue.Count > 0) {
            var node = queue.Remove();

            if (!GetBlockState(node).Settings.IsNonSolid)
                continue;
            
            connected.Add(node);

            for (int i = 0; i < 3; i++) {
                if (node[i] > 0) {
                    var newNode = node;
                    newNode[i] -= 1;
                    if (!connected.Contains(newNode))
                        queue.Add(newNode);
                }
                if (node[i] < PositionExtensions.ChunkSize) {
                    var newNode = node;
                    newNode[i] += 1;
                    if (!connected.Contains(newNode))
                        queue.Add(newNode);
                }
            }
        }
        
        return connected;
    }

    public virtual void Tick() {
        // Update deffered lists.
        TickList.UpdateCollection();

        // Tick all the tickables in this chunk.
        foreach (var tickable in TickList)
            ProcessTickable(tickable);
    }

    public virtual void ProcessTickable(Tickable t) {
        t.Tick();
    }

    public void AddTickable(Tickable tickable)
        => TickList.Add(tickable);

    public void RemoveTickable(Tickable tickable)
        => TickList.Remove(tickable);

    public void Dispose()
        => storage.Dispose();

    internal void IncrementViewCount()
        => viewCount++;

    internal void DecrementViewCount() {
        if (viewCount != 0)
            viewCount--;

        if (viewCount == 0)
            World.UnloadChunk(ChunkPosition);
    }

    internal void AddEntity<T>(T toAdd) where T : Entity {
        Entities.Add(toAdd);
    }
    internal void RemoveEntity<T>(T toRemove) where T : Entity {
        Entities.Remove(toRemove);
    }
}
