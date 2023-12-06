namespace Common.Network.Packets;

/// <summary>
/// Clean way of handling multiple packets by type.
/// </summary>
public class PacketHandler {

    private readonly Dictionary<Type, Action<Packet>> Handlers = new();

    public void RegisterHandler<T>(Action<T> handler) where T : Packet => Handlers[typeof(T)] = packet => handler((T)packet);

    public bool HandlePacket(Packet packet) {
        var type = packet.GetType();

        if (!Handlers.TryGetValue(type, out var handler))
            return false;

        handler(packet);

        return true;
    }
}
