using System.Collections.Concurrent;

namespace Voxel.Common.Network.Packets.Utils;

public static class PacketPool {
    private static Dictionary<Type, ConcurrentQueue<Packet>> Pools = new();

    public static T GetPacket<T>() where T : Packet {
        var baseType = typeof(T);

        if (!Pools.TryGetValue(baseType, out var pool))
            Pools[baseType] = pool = new ConcurrentQueue<Packet>();

        if (!pool.TryDequeue(out var packet))
            packet = Activator.CreateInstance<T>();

        return (T)packet;
    }

    public static T GetPacket<T>(Type targetType) where T : Packet {
        var baseType = typeof(T);

        if (!targetType.IsAssignableTo(baseType))
            throw new InvalidOperationException($"Cannot cast type {targetType} to {baseType}");

        if (!Pools.TryGetValue(targetType, out var pool))
            Pools[targetType] = pool = new ConcurrentQueue<Packet>();

        if (!pool.TryDequeue(out var packet))
            packet = (Packet)Activator.CreateInstance(targetType);

        return (T)packet;
    }

    public static void Return(Packet toReturn) {
        var type = toReturn.GetType();
        if (!Pools.TryGetValue(type, out var pool))
            Pools[type] = pool = new ConcurrentQueue<Packet>();

        toReturn.OnReturnToPool();
        pool.Enqueue(toReturn);
    }
}
