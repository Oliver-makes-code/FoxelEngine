using Foxel.Common.Network.Packets.Utils;
using Foxel.Common.Util;
using Foxel.Common.World.Content.Blocks.State;
using Greenhouse.Libs.Serialization;

namespace Foxel.Common.Network.Packets.C2S.Gameplay.Actions;

public class PlaceBlockC2SPacket : PlayerActionC2SPacket {
    public static readonly Codec<PlaceBlockC2SPacket> Codec = RecordCodec<PlaceBlockC2SPacket>.Create(
        FoxelCodecs.DVec3.Field<PlaceBlockC2SPacket>("position", it => it.position),
        FoxelCodecs.DVec2.Field<PlaceBlockC2SPacket>("rotation", it => it.rotation),
        BlockState.Codec.Field<PlaceBlockC2SPacket>("state", it => it.state),
        (position, rotation, state) => {
            var pkt = PacketPool.GetPacket<PlaceBlockC2SPacket>();
            pkt.position = position;
            pkt.rotation = rotation;
            pkt.state = state;
            return pkt;
        }
    );
    public static readonly Codec<Packet> ProxyCodec = new PacketProxyCodec<PlaceBlockC2SPacket>(Codec);

    public BlockState state;

    public override Codec<Packet> GetCodec()
        => ProxyCodec;
}
