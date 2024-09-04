using Foxel.Common.World.Content.Entities.Player;
using Foxel.Core.Util;
using Greenhouse.Libs.Serialization;

namespace Foxel.Common.World.Content.Entities;

public static class EntityStore {
    public static readonly Codec<Entity> Player = PlayerEntity.ProxyCodec;


    private static void RegisterEntityCodec(ResourceKey key, Codec<Entity> entity) {
        ContentStores.Entitycodecs.Register(key, entity);
    }

    internal static void RegisterStaticContent() {
        RegisterEntityCodec(new("player"), Player);
    }
}
