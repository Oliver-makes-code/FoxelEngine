using Voxel.Common.Network.Packets.Utils;

namespace Voxel.Common.Network.Packets;

/// <summary>
/// Clean way of handling multiple packets by type.
/// </summary>
public class PacketHandler<T> where T : Packet {

    private readonly Dictionary<Type, Action<T>> Handlers = new();

    public void RegisterHandler<T2>(Action<T2> handler) where T2 : T
        => Handlers[typeof(T2)] = packet => handler((T2)packet);

    public bool HandlePacket(T packet) {
        var type = packet.GetType();

        if (!Handlers.TryGetValue(type, out var handler))
            return false;

        handler(packet);

        PacketPool.Return(packet);
        return true;
    }
}
