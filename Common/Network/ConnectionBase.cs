using Common.Network.Packets;
using NLog.Targets;

namespace Common.Network;

public abstract class ConnectionBase<T> where T : Packet {

    public bool isDead { get; private set; } = false;

    public event Action OnClosed = () => {};

    public PacketHandler<T>? packetHandler;

    /// <summary>
    /// Sends a packet to the other side of the connection
    /// </summary>
    /// <param name="toSend"></param>
    public abstract void DeliverPacket(Packet toSend);

    /// <summary>
    /// Closes the connection by authority of the server.
    /// </summary>
    public void Close() {
        isDead = true;
        OnClosed();
    }

    public void HandlePacket(T packet) {
        packetHandler?.HandlePacket(packet);
    }
}
