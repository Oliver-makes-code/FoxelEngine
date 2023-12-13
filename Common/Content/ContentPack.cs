using Voxel.Common.Tile;
using Voxel.Common.World.Entity;
using Voxel.Common.Network.Packets;

namespace Voxel.Common.Content;

/// <summary>
/// Holds all of the content related to some part of the game.
///
/// Ideally in the future will be transmittable across network.
/// </summary>
public class ContentPack {
    public readonly string ID;

    public readonly Dictionary<string, Block> Blocks = new();
    public readonly Dictionary<string, Type> EntityTypes = new();
    public readonly Dictionary<string, Type> PacketTypes = new();

    public ContentPack(string id) {
        ID = id;
    }

    public virtual void Load() {

    }
    
    public T AddBlock<T>(T b) where T : Block {
        Blocks[b.Name] = b;
        return b;
    }

    public void AddPacketType<T>(string name) where T : Packet {
        PacketTypes[name] = typeof(T);
    }

    public void AddEntityType<T>(string name) where T : Entity {
        EntityTypes[name] = typeof(T);
    }
}
