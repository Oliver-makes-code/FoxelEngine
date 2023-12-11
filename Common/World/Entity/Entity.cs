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
    public dvec2 rotation = dvec2.Zero;

    public dvec3 lastPosition { get; private set; }
    public dvec2 lastRotation { get; private set; }

    public dvec3 velocity { get; set; }
    public bool isOnFloor { get; private set; }

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

    public dvec3 eyeOffset => dvec3.UnitY * (eyeHeight - boundingBox.size.y * 0.5);
    
    public void AddToWorld(VoxelWorld newWorld, dvec3 pos, dvec2 rot) {
        world = newWorld;
        position = pos;
        rotation = rot;
        OnAddedToWorld();
    }

    public virtual void OnAddedToWorld() {}

    public virtual void Tick() {
        lastPosition = position;
        lastRotation = rotation;
        
        velocity = MoveAndSlide(velocity * Constants.SecondsPerTick) * Constants.TicksPerSecond;
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

    public dvec3 MoveAndSlide(dvec3 movement)
        => position += PhysicsSim.MoveAndSlide(boundingBox.Translated(position), movement, world);
    
    public dvec3 SmoothPosition(float delta)
        => dvec3.Lerp(lastPosition, position, delta);
    public dvec2 SmoothRotation(float delta)
        => dvec2.Lerp(lastRotation, rotation, delta);
}
