using System.Reflection;
using Voxel.Common.Util.Serialization;

namespace Voxel.Common.Network.Packets.Utils;

public class PacketMap<T> : RawIDMap<Type> where T : Packet {
    public void FillOutgoingMap() {
        var types = Assembly.GetAssembly(typeof(PacketMap<>))?.ExportedTypes;

        if (types == null)
            return;

        var baseType = typeof(T);
        var lastId = 0u;

        foreach (var type in types) {
            if (!type.IsAssignableTo(baseType))
                continue;

            outgoingMap[type] = lastId++;

            //Console.WriteLine($"Found packet {type.Name} assignable to {baseType.Name}");
        }
    }


    public override void WriteOutgoingMap(VDataWriter writer) {
        writer.Write(outgoingMap.Count);

        foreach ((var key, uint value) in outgoingMap) {
            writer.Write(key.FullName);
            writer.Write(value);
        }
    }
    public override void ReadIncomingMap(VDataReader reader) {
        var assembly = Assembly.GetAssembly(typeof(PacketMap<>));

        var count = reader.ReadInt();
        for (int i = 0; i < count; i++) {
            var name = reader.ReadString();
            var value = reader.ReadUint();

            var type = assembly?.GetType(name);

            if (type == null) {
                //TODO - Log error! Unexpected packet.
                continue;
            }

            incomingMap[value] = type;
        }
    }
}
