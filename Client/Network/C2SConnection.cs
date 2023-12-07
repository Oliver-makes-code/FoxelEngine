using Common.Network;
using Common.Network.Packets.S2C;

namespace Voxel.Client.Network;

public abstract class C2SConnection : ConnectionBase<S2CPacket> {

    public abstract void Tick();
}
