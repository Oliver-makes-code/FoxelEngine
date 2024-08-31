using Foxel.Common.Tile;
using Foxel.Common.World.Entity;
using Foxel.Common.Network.Packets;
using Foxel.Core.Util;

namespace Foxel.Common.Content;

/// <summary>
/// Holds all of the content related to some part of the game.
///
/// Ideally in the future will be transmittable across network.
/// </summary>
public class ContentPack {
    public readonly string Id;

    public readonly Dictionary<ResourceKey, Block> Blocks = [];
    public readonly Dictionary<ResourceKey, Type> EntityTypes = [];
    public readonly Dictionary<ResourceKey, Type> PacketTypes = [];

    public ContentPack(string id) {
        Id = id;
    }

    public virtual void Load() {

    }
    
    public T AddBlock<T>(T b) where T : Block {
        Blocks[b.Name] = b;
        return b;
    }

    public void AddPacketType<T>(ResourceKey name) where T : Packet {
        PacketTypes[name] = typeof(T);
    }

    public void AddEntityType<T>(ResourceKey name) where T : Entity {
        EntityTypes[name] = typeof(T);
    }
}
