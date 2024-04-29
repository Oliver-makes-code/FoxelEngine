using Voxel.Common.Util;

namespace Voxel.Common.World.Components;

public abstract class Component<TEntity> {
    public abstract void Register(TEntity entity);

    public void Setup(TEntity entity)
        => SetupInternal(entity);

    private protected virtual void SetupInternal(TEntity entity) {}
}

public abstract class Component<TEntity, TSettings> : Component<TEntity> {
    public readonly SerializedData<TSettings> Settings;

    public Component(SerializedData<TSettings> settings) {
        Settings = settings;
    }

    private protected override void SetupInternal(TEntity entity) {
        // TODO: Deserialize Settings
    }
}

public abstract class Component<TEntity, TSettings, TData> : Component<TEntity, TSettings> {
    public readonly SerializedData<TData> Data;

    public Component(SerializedData<TSettings> settings, SerializedData<TData> data) : base(settings) {
        Data = data;
    }

    private protected override void SetupInternal(TEntity entity) {
        base.SetupInternal(entity);

        // TODO: Deserialize Data
    }
}
