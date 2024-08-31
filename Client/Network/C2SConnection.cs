using Foxel.Common.Network;
using Foxel.Common.Network.Packets.S2C;

namespace Foxel.Client.Network;

public abstract class C2SConnection : ConnectionBase<S2CPacket> {

    public abstract void Tick();
}
