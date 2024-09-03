using GlmSharp;
using Foxel.Common.Util.Serialization;
using Foxel.Common.World.Entity;
using Greenhouse.Libs.Serialization;
using Foxel.Common.Util;
using Foxel.Common.Network.Packets.Utils;

namespace Foxel.Common.Network.Packets.C2S.Gameplay.Actions;

public abstract class PlayerActionC2SPacket : C2SPacket {
    public dvec3 position;
    public dvec2 rotation;

    public virtual void Init(Entity entity) {
        position = entity.position;
        rotation = entity.rotation;
    }
}
