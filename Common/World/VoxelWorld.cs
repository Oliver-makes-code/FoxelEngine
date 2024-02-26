using System.Diagnostics.CodeAnalysis;
using System.Transactions;
using GlmSharp;
using Voxel.Common.Collision;
using Voxel.Common.Content;
using Voxel.Common.Server;
using Voxel.Common.Tile;
using Voxel.Common.Util;
using Voxel.Common.Util.Profiling;
using Voxel.Common.World.Tick;
using Voxel.Common.World.Views;
using Voxel.Core.Util;

namespace Voxel.Common.World;

public abstract class VoxelWorld : BlockView, ColliderProvider {

    private static readonly Profiler.ProfilerKey TickKey = Profiler.GetProfilerKey("World Tick");

    private readonly Dictionary<ivec3, Chunk> Chunks = new();
    private readonly List<Box> CollisionShapeCache = new();

    public readonly TickList GlobalTickables = new();

    public readonly Random Random = new();

    private readonly DefferedList<Entity.Entity> WorldEntities = new();
    private readonly Dictionary<Guid, Entity.Entity> EntitiesByID = new();

    public bool TryGetChunkRaw(ivec3 chunkPos, [NotNullWhen(true)] out Chunk? chunk) => Chunks.TryGetValue(chunkPos, out chunk);
    public bool TryGetChunk(dvec3 worldPosition, [NotNullWhen(true)] out Chunk? chunk) => TryGetChunkRaw(worldPosition.WorldToChunkPosition(), out chunk);
    public bool TryGetChunk(ivec3 blockPosition, [NotNullWhen(true)] out Chunk? chunk) => TryGetChunkRaw(blockPosition.BlockToChunkPosition(), out chunk);

    public virtual bool IsChunkLoadedRaw(ivec3 chunkPos) => Chunks.ContainsKey(chunkPos);


    /// <summary>
    /// Gets or creates a chunk at a given position.
    ///
    /// If the chunk did not exist, the new chunk is empty.
    ///
    /// Ideally, should only be used by terrain generator or when client gets packets from server.
    /// </summary>
    /// <param name="chunkPosition">The chunk-space position of the chunk we're trying to get or create.</param>
    /// <returns></returns>
    public Chunk GetOrCreateChunk(ivec3 chunkPosition) {
        if (Chunks.TryGetValue(chunkPosition, out var chunk))
            return chunk;

        chunk = CreateChunk(chunkPosition);

        Chunks[chunkPosition] = chunk;
        GlobalTickables.Add(chunk);
        return chunk;
    }

    protected virtual Chunk CreateChunk(ivec3 pos) => new(pos, this);

    internal void UnloadChunk(ivec3 chunkPosition) {
        Chunks.Remove(chunkPosition, out var c);

        if (c == null)
            return;

        GlobalTickables.Remove(c);
        c.storage.Dispose();

        //Remove entities that were a part of that chunk from the global entity list.
        foreach (var entity in c.Entities)
            WorldEntities.Remove(entity);
    }

    internal ChunkView GetOrCreateChunkView(ivec3 chunkPosition) {
        var chunk = GetOrCreateChunk(chunkPosition);
        return new(chunk);
    }

    public void SetBlock(ivec3 position, Block block) {
        var chunkPos = position.BlockToChunkPosition();
        if (!TryGetChunkRaw(chunkPos, out var chunk))
            return;

        OnBlockChanged(position, block);
        var lPos = position - chunk.WorldPosition;
        chunk.SetBlock(lPos, block);
    }

    protected virtual void OnBlockChanged(ivec3 position, Block newBlock) {

    }

    public Block GetBlock(ivec3 position) {
        var chunkPos = position.BlockToChunkPosition();
        if (!TryGetChunkRaw(chunkPos, out var chunk))
            return MainContentPack.Instance.Air;

        var lPos = position - chunk.WorldPosition;
        return chunk.GetBlock(lPos);
    }

    public List<Box> GatherColliders(Box box) {
        var min = (ivec3)dvec3.Floor(box.min);
        var max = (ivec3)dvec3.Ceiling(box.max);

        CollisionShapeCache.Clear();

        var half = dvec3.Ones * 0.5;

        foreach (var pos in Iteration.Cubic(min, max)) {
            var chunkPos = pos.BlockToChunkPosition();

            if (!IsChunkLoadedRaw(chunkPos) || !GetBlock(pos).IsAir)
                CollisionShapeCache.Add(Box.FromPosSize(pos + half, dvec3.Ones));
        }

        return CollisionShapeCache;
    }

    public virtual void AddEntity(Entity.Entity entity, dvec3 position, dvec2 rotation) {
        if (!TryGetChunkRaw(position.WorldToChunkPosition(), out var chunk))
            throw new InvalidOperationException("Cannot add entity to chunk that doesn't exist");

        //Notify entity that it was added to the world.
        entity.chunk = chunk;
        entity.AddedToWorld(this, position, rotation);

        //Add entity to list of all loaded entities and add it to the chunk it belongs to.
        WorldEntities.Add(entity);
        EntitiesByID[entity.ID] = entity;
        chunk.AddEntity(entity);
    }

    /// <summary>
    /// Called to remove the entity from the world.
    /// </summary>
    /// <param name="entity"></param>
    public virtual void RemoveEntity(Entity.Entity entity) {
        WorldEntities.Remove(entity);
        EntitiesByID.Remove(entity.ID);
        entity.chunk.RemoveEntity(entity);

        VoxelServer.Logger.Info($"Unloading Entity {entity}");
    }

    public virtual bool TryGetEntity(Guid id, [NotNullWhen(true)] out Entity.Entity? entity) => EntitiesByID.TryGetValue(id, out entity);

    public void Tick() {
        using (TickKey.Push()) {
            //Update chunks and other tickables.
            GlobalTickables.UpdateCollection();
            foreach (var tickable in GlobalTickables)
                tickable.Tick();

            //Update all entities.
            WorldEntities.UpdateCollection();
            foreach (var entity in WorldEntities) {
                //Process entity.
                ProcessEntity(entity);

                //Move entity to new chunk if required.
                if (entity.chunk.ChunkPosition != entity.chunkPosition) {
                    //If new chunk does not exist, unload entity.
                    if (!TryGetChunkRaw(entity.chunkPosition, out var chunk)) {
                        RemoveEntity(entity);
                        return;
                    }

                    //Move entity to new chunk.
                    entity.chunk.RemoveEntity(entity);
                    entity.chunk = chunk;
                    chunk.AddEntity(entity);

                    //Console.WriteLine($"Moving entity {entity.ID} to chunk {entity.chunkPosition}");
                }
            }

            Profiler.SetCurrentMeta($"{GlobalTickables.Count} tickables, {WorldEntities.Count} entities");
        }
    }

    public virtual void ProcessEntity(Entity.Entity e) {
        if (e is Tickable t) t.Tick();
    }

    public void Dispose() {
        foreach (var chunk in Chunks.Values)
            chunk.Dispose();
    }
}
