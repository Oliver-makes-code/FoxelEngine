using Voxel.Common.Util.Registration;

namespace Voxel.Common.Content;

public class ContentDatabase {
    public static readonly ContentDatabase Instance = new();

    public readonly Registries Registries = new();

    public void LoadPack(ContentPack pack) {
        pack.Load();

        foreach ((string id, var type) in pack.PacketTypes)
            Registries.PacketTypes.Register(id, type);

        foreach ((string id, var block) in pack.Blocks)
            Registries.Blocks.Register(block, id);
        foreach ((string id, var type) in pack.EntityTypes)
            Registries.EntityTypes.Register(id, type);
    }

    public void Finish() {
        Registries.GenerateIds();

        //Update blocks with internal IDs
        foreach ((var entry, string? id, uint raw) in Registries.Blocks.Entries())
            entry.id = raw;
    }

    public void Clear() {
        Registries.Clear();
    }
}
