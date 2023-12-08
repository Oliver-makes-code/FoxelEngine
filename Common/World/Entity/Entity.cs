using Common.Network.Packets;
using Common.Network.Packets.S2C.Gameplay;
using Common.Network.Packets.Utils;
using GlmSharp;
using Voxel.Common.Collision;
using Voxel.Common.Util;

namespace Voxel.Common.World.Entity;

public abstract class Entity : Tickable {
    public VoxelWorld world { get; private set; }
    public Chunk chunk { get; private set; }

    public Guid ID = Guid.Empty;

    /// <summary>
    /// World-space 3d full position.
    /// </summary>
    public dvec3 position = new(0, 0, 0);
    /// <summary>
    /// World-space rotation of entity around the y axis.
    /// </summary>
    public float rotation = 0;
    
    /// <summary>
    /// World-space 3d block position of the entity.
    /// </summary>
    public ivec3 blockPosition {
        get => (ivec3)dvec3.Floor(position);
        set => position = value;
    }

    /// <summary>
    /// The chunk-space 3d position of the entity.
    /// </summary>
    public ivec3 chunkPosition => blockPosition.BlockToChunkPosition();

    public bool destroyed { get; private set; } = false;

    public abstract float eyeHeight { get; }
    public abstract AABB boundingBox { get; }

    public void AddToWorld(VoxelWorld newWorld, dvec3 pos, float rot) {
        world = newWorld;
        position = pos;
        rotation = rot;
        OnAddedToWorld();
    }

    public virtual void OnAddedToWorld() {}

    public virtual void Tick() {
        
    }

    /// <summary>
    /// Queues an entity to be destroyed at the end of the tick.
    /// </summary>
    public void Destroy() {
        destroyed = true;
    }

    /// <summary>
    /// Cancels the destruction of the entity at the end of the tick.
    /// </summary>
    public void CancelDestroy() {
        destroyed = false;
    }


    /// <summary>
    /// Actually destroys the entity, 
    /// </summary>
    public void TrueDestroy() {

    }
}
