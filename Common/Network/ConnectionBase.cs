using Common.Network.Packets;

namespace Common.Network;

public abstract class ConnectionBase {

    public bool isDead { get; protected set; } = false;

    public event Action<Packet> OnPacket = _ => {};

    protected void OnRecievedPacket(Packet packet) => OnPacket(packet);

    /// <summary>
    /// Sends a packet to the other side of the connection
    /// </summary>
    /// <param name="toSend"></param>
    public abstract void DeliverPacket(Packet toSend);

    /// <summary>
    /// Checks for any new packets and calls OnRecievedPacket for each of them.
    /// </summary>
    public abstract void Poll(PacketHandler packetHandler);

    /// <summary>
    /// Closes the connection by authority of the server.
    /// </summary>
    public abstract void Close();
}
