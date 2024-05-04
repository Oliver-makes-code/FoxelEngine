using Newtonsoft.Json.Linq;

namespace Voxel.Common.World.Ecs;

public interface EcsSystem<TSelf, TEntity, TInstance, TBuilder>
where TSelf : EcsSystem<TSelf, TEntity, TInstance, TBuilder>
where TEntity : EcsEntity<TEntity, TInstance, TBuilder>
where TInstance : EcsEntityInstance<TEntity, TInstance, TBuilder> 
where TBuilder : EcsEntityBuilder<TEntity, TInstance, TBuilder> {
    public void Register(TBuilder builder);
}
