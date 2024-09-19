using System;
using System.Collections.Generic;
using GlmSharp;
using Foxel.Client.Rendering.Texture;
using Foxel.Client.Rendering.VertexTypes;
using Foxel.Core.Rendering;
using Foxel.Core.Util;
using Foxel.Common.Util;
using Foxel.Common.World.Content.Items;
using Foxel.Common.World.Content;

namespace Foxel.Client.Rendering.Gui;

public class GuiBuilder {
    public delegate void LayerConsumer(LayerBuilder consumer);

    private readonly HashSet<ResourceKey> LayerKeySet = [];
    private readonly List<ResourceKey> OrderedLayers = [];
    private readonly Dictionary<ResourceKey, LayerConsumer> ConsumersById = [];
    private readonly VertexConsumer<GuiQuadVertex> VertexConsumer = new();

    private bool shouldRebuild = false;

    public void Clear() {
        LayerKeySet.Clear();
        OrderedLayers.Clear();
        ConsumersById.Clear();
        shouldRebuild = true;
    }

    public void AddLayer(ResourceKey key, LayerConsumer consumer) {
        ConsumersById[key] = consumer;
        shouldRebuild = true;
        if (LayerKeySet.Add(key))
            OrderedLayers.Add(key);
    }

    public void MarkForRebuild()
        => shouldRebuild = true;

    public void BuildAll(Atlas atlas, Action<VertexConsumer<GuiQuadVertex>> upload) {
        if (!shouldRebuild)
            return;
        shouldRebuild = false;
        VertexConsumer.Clear();

        var builder = new LayerBuilder(atlas, VertexConsumer);

        foreach (var key in OrderedLayers)
            ConsumersById[key](builder);
        
        upload(VertexConsumer);
    }

    public class LayerBuilder {
        private readonly Atlas Atlas;
        private readonly VertexConsumer<GuiQuadVertex> Consumer;

        public LayerBuilder(Atlas atlas, VertexConsumer<GuiQuadVertex> consumer) {
            Atlas = atlas;
            Consumer = consumer;
        }

        public void Add(GuiQuadVertex vertex)
            => Consumer.Vertex(vertex);

        public GuiQuadVertex Model(ResourceKey model) {
            return Sprite(model.WithGroup("model/"))
                .WithSize(new(48, 48));
        }

        public GuiQuadVertex Item(ItemStack stack) {
            // TODO: Create an ItemModelManager.
            return Sprite(ContentStores.Items.GetKey(stack.Item).PrefixValue("models/item/"))
                .WithSize(new(48, 48));
        }

        public GuiQuadVertex Sprite(ResourceKey? sprite) {
            if (!Conditions.IsNonNull(sprite, out var acutalSprite) || !Atlas.TryGetSprite(acutalSprite, out var texture))
                return SizeAndColor(new(16, 16), new(1, 0, 0, 1));
            return new() {
                color = new(1, 1, 1, 1),
                size = texture.size,
                uvMin = texture.uvPosition,
                uvMax = texture.uvPosition + texture.uvSize
            };
        }

        public GuiQuadVertex SizeAndColor(ivec2 size, vec4 color)
            => new() {
                size = size,
                color = color,
                uvMin = new(-1, -1),
                uvMax = new(-1, -1),
            };
    }
}
