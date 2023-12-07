using Common.Network.Packets;
using Common.Network.Packets.C2S;
using Common.Network.Packets.S2C;

namespace Common.Network;

/// <summary>
/// Connection out from the server to another client.
/// </summary>
public abstract class S2CConnection : ConnectionBase<C2SPacket> {
}
