using GlmSharp;
using Foxel.Common.Collision;
using Foxel.Common.Util;
using Greenhouse.Libs.Serialization;

namespace Foxel.Common.World.Content.Entities;

public abstract class Entity {
    public VoxelWorld? world { get; private set; }
    public Chunk? chunk { get; internal set; }

    public Guid id = Guid.Empty;

    /// <summary>
    /// World-space 3d full position.
    /// </summary>
    public dvec3 position = new(0, 0, 0);
    /// <summary>
    /// World-space rotation of entity around the y axis.
    /// </summary>
    public dvec2 rotation = dvec2.Zero;

    public dvec3 lastPosition { get; internal set; }
    public dvec2 lastRotation { get; internal set; }

    public dvec3 velocity { get; set; }
    public bool isOnFloor { get; internal set; }

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
    public abstract Box boundingBox { get; }

    public dvec3 eyeOffset => dvec3.UnitY * (eyeHeight - boundingBox.size.y * 0.5);

    public Entity() {}

    public void AddedToWorld(VoxelWorld newWorld, dvec3 pos, dvec2 rot) {
        world = newWorld;
        position = pos;
        rotation = rot;
        OnAddedToWorld();
    }

    public virtual void OnAddedToWorld() {}

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
    public void TrueDestroy() {}

    // TODO: Implement logic
    public void MarkDirty() {}

    public dvec3 MoveAndSlide(dvec3 movement, out dvec3 translateBy) {
        translateBy = new(0, 0, 0);
        return world != null
        ? PhysicsSim.MoveAndSlide(boundingBox.Translated(position), movement * Constants.SecondsPerTick, world, isOnFloor, out translateBy) * Constants.TicksPerSecond
        : new(0, 0, 0);
    }

    public bool CalculateIsOnFloor()
        => PhysicsSim.CastBox(boundingBox.Translated(position), new(0, -2 * PhysicsSim.Epsilon, 0), world!, out _);

    public dvec3 SmoothPosition(float delta)
        => dvec3.Lerp(lastPosition, position, delta);
        
    public dvec2 SmoothRotation(float delta)
        => dvec2.Lerp(lastRotation, rotation, delta);

    public abstract Codec<Entity> GetCodec();
}

public record EntityProxyCodec<TEntity>(Codec<TEntity> Codec) : Codec<Entity> where TEntity : Entity {
    public override Entity ReadGeneric(DataReader reader)
        => Codec.ReadGeneric(reader);
    public override void WriteGeneric(DataWriter writer, Entity value)
        => Codec.WriteGeneric(writer, (TEntity) value);
}

