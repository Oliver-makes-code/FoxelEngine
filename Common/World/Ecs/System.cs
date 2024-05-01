namespace Voxel.Common.World.Ecs;

public interface EcsSystem<TEntity> where TEntity : EcsEntity<TEntity> {
    public void Register(EcsEntityBuilder<TEntity> builder);
}
