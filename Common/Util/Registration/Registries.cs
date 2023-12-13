using Voxel.Common.Network.Packets;
using Voxel.Common.Tile;
using Voxel.Common.Util.Serialization;
using Voxel.Common.World.Entity;

namespace Voxel.Common.Util.Registration;

public class Registries {
    private Dictionary<string, BaseRegistry> RegistryList = new();

    public readonly SimpleRegistry<Block> Blocks = new();
    public readonly TypedRegistry<Entity> EntityTypes = new();

    public readonly TypedRegistry<Packet> PacketTypes = new();

    public Registries() {
        AddRegistry("packet_type", PacketTypes);

        AddRegistry("block", Blocks);
        AddRegistry("entity_type", EntityTypes);
    }

    public void AddRegistry<T>(string name, T toAdd) where T : BaseRegistry => RegistryList[name] = toAdd;

    public void WriteSync(VDataWriter writer) {
        writer.Write(RegistryList.Count);

        foreach ((string? name, var baseReg) in RegistryList) {
            writer.Write(name);
            baseReg.Write(writer);
        }
    }

    public void ReadSync(VDataReader reader) {
        var count = reader.ReadInt();

        for (int i = 0; i < count; i++) {
            var name = reader.ReadString();

            if (!RegistryList.TryGetValue(name, out var reg))
                throw new InvalidOperationException($"Unrecognized registry {name}!");

            reg.Read(reader);
        }
    }

    public void Clear() {
        foreach (var value in RegistryList.Values)
            value.Clear();
    }
    public void GenerateIDs() {
        foreach (var value in RegistryList.Values)
            value.GenerateIDs();
    }
}
