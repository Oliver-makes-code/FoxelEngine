using Foxel.Core.Util;
using Foxel.Common.Util.Registration;

namespace Foxel.Common.Content;

public class ContentDatabase {
    public static readonly ContentDatabase Instance = new();

    public readonly Registries Registries = new();

    public void LoadPack(ContentPack pack) {
        pack.Load();

        foreach ((ResourceKey id, var block) in pack.Blocks)
            Registries.Blocks.Register(block, id);
    }

    public void Finish() {
        Registries.GenerateIds();

        //Update blocks with internal IDs
        foreach ((var entry, ResourceKey id, uint raw) in Registries.Blocks.Entries())
            entry.id = raw;
    }

    public void Clear() {
        Registries.Clear();
    }
}
