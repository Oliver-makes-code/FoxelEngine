using GlmSharp;
using Voxel.Common.Content;
using Voxel.Common.Tile;
using Voxel.Common.Util;
using Voxel.Common.World.Storage;
using Voxel.Common.World.Tick;
using Voxel.Core.Util;

namespace Voxel.Common.World;

public class Chunk : Tickable, IDisposable {
    public readonly ivec3 ChunkPosition;
    public readonly ivec3 WorldPosition;

    public readonly VoxelWorld World;

    public readonly TickList TickList = new();
    public readonly List<Entity.Entity> Entities = new();

    public ChunkStorage storage { get; private set; }

    private uint viewCount;

    /// <summary>
    /// When a chunk is modified, this is incremented. This is used to tell systems like networking or chunk meshing that the chunk has changed.
    /// </summary>
    private uint _version;

    public bool IsEmpty => storage is SingleStorage ss && ss.Block == MainContentPack.Instance.Air;

    public Chunk(ivec3 chunkPosition, VoxelWorld world, ChunkStorage? storage = null) {
        this.storage = storage ?? new SingleStorage(MainContentPack.Instance.Air, this);

        ChunkPosition = chunkPosition;
        WorldPosition = ChunkPosition * PositionExtensions.ChunkSize;
        World = world;
    }

    public ChunkStorage CopyStorage()
        => storage.GenerateCopy();

    public virtual void SetStorage(ChunkStorage newStorage) {
        storage.Dispose();
        storage = newStorage;
        IncrementVersion();
    }

    public void SetBlock(ivec3 position, Block toSet) {
        storage[position] = toSet;
        IncrementVersion();
    }
    public Block GetBlock(ivec3 position)
        => storage[position];

    public uint GetVersion()
        => _version;

    public void IncrementVersion() {
        Interlocked.Increment(ref _version);
    }
    
    // I don't know if this is the best place to put this..
    public HashSet<ivec3> FloodFill(ivec3 root) {
        var connected = new HashSet<ivec3>();

        if (storage is SingleStorage singleStorage) {
            if (singleStorage.Block.IsAir)
                foreach (var pos in Iteration.Cubic(0, PositionExtensions.ChunkSize))
                    connected.Add(pos);
            return connected;
        }
        
        var queue = new Stack<ivec3>();
        queue.Push(root.Loop(PositionExtensions.ChunkSize));
        while (queue.Count > 0) {
            var node = queue.Pop();

            if (!GetBlock(node).IsAir)
                continue;
            
            connected.Add(node);

            for (int i = 0; i < 3; i++) {
                if (node[i] > 0) {
                    var newNode = node;
                    newNode[i] -= 1;
                    if (!connected.Contains(newNode))
                        queue.Push(newNode);
                }
                if (node[i] < PositionExtensions.ChunkSize) {
                    var newNode = node;
                    newNode[i] += 1;
                    queue.Push(newNode);
                    if (!connected.Contains(newNode))
                        queue.Push(newNode);
                }
            }
        }
        
        return connected;
    }

    internal void IncrementViewCount() {
        viewCount++;
    }

    internal void DecrementViewCount() {
        if (viewCount != 0)
            viewCount--;

        if (viewCount == 0)
            World.UnloadChunk(ChunkPosition);
    }


    public void Tick() {
        //Update deffered lists.
        TickList.UpdateCollection();

        //Tick all the tickables in this chunk.
        foreach (var tickable in TickList)
            ProcessTickable(tickable);
    }

    public virtual void ProcessTickable(Tickable t) {
        t.Tick();
    }

    public void AddTickable(Tickable tickable) => TickList.Add(tickable);
    public void RemoveTickable(Tickable tickable) => TickList.Remove(tickable);

    internal void AddEntity<T>(T toAdd) where T : Entity.Entity {
        Entities.Add(toAdd);
    }
    internal void RemoveEntity<T>(T toRemove) where T : Entity.Entity {
        Entities.Remove(toRemove);
    }

    public void Dispose() {
        storage.Dispose();
    }
}
