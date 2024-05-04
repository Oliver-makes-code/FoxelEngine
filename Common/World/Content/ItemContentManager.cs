using CSScripting;
using Newtonsoft.Json.Linq;
using Voxel.Common.World.Items;
using Voxel.Common.World.Items.System;
using Voxel.Core.Util;

namespace Voxel.Common.World.Content;

public class ItemContentManager : ServerContentManager<ItemContentManager.ItemJson, Item> {
    private static readonly Dictionary<ResourceKey, Func<JObject?, ItemSystem?>> Systems = new() {
        [new("block_item")] = BlockItemSystem.Create
    };

    public override string ContentDir()
        => "items/";
    
    public override Item Load(ResourceKey key, ItemJson json) {
        var builder = new ItemBuilder();

        // TODO: Log errors
        foreach (var systemJson in json.systems ?? []) {
            if (systemJson.id == null)
                continue;
            
            var systemConstructor = Systems.TryGetValue(new(systemJson.id), out var value) ? value : null;

            if (systemConstructor == null)
                continue;

            var system = systemConstructor(systemJson.options);

            if (system == null)
                continue;
            
            system.Register(builder);
        }

        return new Item(builder);
    }

    public class ItemJson {
        public SystemJson[]? systems;
    }

    public class SystemJson {
        public string? id;
        public JObject? options;
    }
}
