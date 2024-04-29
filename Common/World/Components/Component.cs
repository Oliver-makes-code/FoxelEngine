using Voxel.Common.Util;

namespace Voxel.Common.World.Components;

public abstract class Component<TEntity> {
    public abstract void Register(TEntity entity);

    public virtual void ReadStatic(TEntity entity) {}
    public virtual void ReadSerialized(TEntity entity) {}
    public virtual void WriteSerialized(TEntity entity) {}
}

public abstract class Component<TEntity, TSettings> : Component<TEntity> {
    public readonly StaticData<TSettings> Settings;

    public Component(StaticData<TSettings> settings) {
        Settings = settings;
    }

    public override void ReadStatic(TEntity entity) {}
}

public abstract class Component<TEntity, TSettings, TData> : Component<TEntity, TSettings> {
    public readonly SerializedData<TData> Data;

    public Component(StaticData<TSettings> settings, SerializedData<TData> data) : base(settings) {
        Data = data;
    }

    public override void ReadSerialized(TEntity entity) {}

    public override void WriteSerialized(TEntity entity) {}
}
