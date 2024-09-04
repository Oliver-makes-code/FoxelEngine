using Foxel.Common.Tile;
using Foxel.Common.World.Content.Entities;
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

    public ContentPack(string id) {
        Id = id;
    }

    public virtual void Load() {

    }
    
    public T AddBlock<T>(T b) where T : Block {
        Blocks[b.Name] = b;
        return b;
    }
}
