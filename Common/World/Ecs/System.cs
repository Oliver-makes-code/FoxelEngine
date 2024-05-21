namespace Voxel.Common.World.Ecs;

public interface EcsSystem<TSelf, TEntity, TBuilder>
where TSelf : EcsSystem<TSelf, TEntity, TBuilder>
where TEntity : EcsEntity<TEntity, TBuilder>
where TBuilder : EcsEntityBuilder<TEntity, TBuilder> {
    public void Register(TBuilder builder);
}
