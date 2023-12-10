using Voxel.Common.Network.Packets.C2S;

namespace Voxel.Common.Network;

/// <summary>
/// Connection out from the server to another client.
/// </summary>
public abstract class S2CConnection : ConnectionBase<C2SPacket> {
}
