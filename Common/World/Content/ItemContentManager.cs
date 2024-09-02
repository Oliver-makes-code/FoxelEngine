using Newtonsoft.Json.Linq;
using Foxel.Common.World.Items;
using Foxel.Common.World.Items.System;
using Foxel.Core.Util;
using Greenhouse.Libs.Serialization;

namespace Foxel.Common.World.Content;

public class ItemContentManager() : ServerContentManager<ItemContentManager.ItemJson, Item>(ItemJson.Codec) {
    private static readonly Dictionary<ResourceKey, Codec<ItemSystem>> SystemCodecs = new() {
        [new("block_item")] = BlockItemSystem.Codec
    };
    private static readonly Codec<ItemSystem> ResolverCodec = new ItemSystemResolverCodec();

    public override string ContentDir()
        => "items/";
    
    public override Item Load(ResourceKey key, ItemJson json) {
        var builder = new ItemBuilder();

        // TODO: Log errors
        foreach (var system in json.Systems ?? []) {
            system.Register(builder);
        }

        var modelJson = json.Model!;

        ResourceKey? modelKey = null;

        if (modelJson?.Type == "block" && modelJson?.Block != null)
            modelKey = new ResourceKey(modelJson.Block).PrefixValue("model/");
        else if (modelJson?.Type == "texture" && modelJson?.Texture != null)
            modelKey = new ResourceKey(modelJson.Texture).PrefixValue("gui/");

        return new Item(builder, modelKey);
    }

    public record ItemJson(
        ItemModel? Model,
        ItemSystem[]? Systems
    ) {
        public static readonly Codec<ItemJson> Codec = RecordCodec<ItemJson>.Create(
            ItemModel.Codec.NullableField<ItemModel, ItemJson>("model", it => it.Model),
            ResolverCodec.Array().NullableField<ItemSystem[], ItemJson>("systems", it => it.Systems),
            (model, systems) => new(model, systems)
        );
    }

    // TODO: make this a separate JSON file.
    public record ItemModel(
        string? Type,
        string? Block,
        string? Texture
    ) {
        public static readonly Codec<ItemModel> Codec = RecordCodec<ItemModel>.Create(
            Codecs.String.NullableField<string, ItemModel>("type", it => it.Type),
            Codecs.String.NullableField<string, ItemModel>("block", it => it.Block),
            Codecs.String.NullableField<string, ItemModel>("texture", it => it.Texture),
            (type, block, tex) => new(type, block, tex)
        );
    }

    public record ItemSystemCodec<TConfig>(Codec<TConfig> ConfigCodec, Func<TConfig, ItemSystem> Constructor) : Codec<ItemSystem> {
        public override ItemSystem ReadGeneric(DataReader reader) {
            var conf = ConfigCodec.ReadGeneric(reader);
            return Constructor(conf);
        }

        public override void WriteGeneric(DataWriter writer, ItemSystem value)
            => throw new("ItemSystemCodec can only be deserialized.");
    }

    private record ItemSystemResolverCodec : Codec<ItemSystem> {
        public override ItemSystem ReadGeneric(DataReader reader) {
            using var obj = reader.Object();
            var id = ResourceKey.Codec.ReadGeneric(obj.Field("id"));
            return SystemCodecs[id].ReadGeneric(obj.Field("options"));
        }

        public override void WriteGeneric(DataWriter writer, ItemSystem value)
            => throw new("ItemSystemResolverCodec can only be deserialized.");
    }
}
