using Foxel.Common.Network.Packets;
using Foxel.Common.Tile;
using Foxel.Common.Util.Serialization;
using Foxel.Common.World.Content.Entities;

namespace Foxel.Common.Util.Registration;

public class Registries {
    private readonly Dictionary<string, BaseRegistry> RegistryList = [];

    public readonly SimpleRegistry<Block> Blocks = new();
    public readonly TypedRegistry<Entity> EntityTypes = new();

    public Registries() {
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
    
    public void GenerateIds() {
        foreach (var value in RegistryList.Values)
            value.GenerateIds();
    }
}
