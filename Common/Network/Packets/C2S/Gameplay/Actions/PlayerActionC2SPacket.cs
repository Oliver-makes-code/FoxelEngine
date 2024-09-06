using GlmSharp;
using Foxel.Common.World.Content.Entities;

namespace Foxel.Common.Network.Packets.C2S.Gameplay.Actions;

public abstract class PlayerActionC2SPacket : C2SPacket {
    public dvec3 position;
    public dvec2 rotation;

    public virtual void Init(Entity entity) {
        position = entity.position;
        rotation = entity.rotation;
    }
}
