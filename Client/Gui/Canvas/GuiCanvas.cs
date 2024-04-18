using System.Collections.Generic;
using GlmSharp;
using Voxel.Client.Rendering.Gui;
using Voxel.Client.Rendering.Texture;
using Voxel.Client.Rendering.VertexTypes;
using Voxel.Core;
using Voxel.Core.Util;

namespace Voxel.Client.Gui.Canvas;

// x and y go from -1 to 1, from the bottom left to the bottom right

public static class GuiCanvas {
    // TODO: Update this as the window changes size
    public static ivec2 ReferenceResolution { get; private set; } = ivec2.Zero;

    public static GuiVertex[] QuadCache {
        get {
            topLayer?.UpdateQuadCache();
            return _QuadCache;
        }
    }
    public static uint QuadCount { get; internal set; }

    // internal so GuiRect.Rebuild() can bypass the needs rebuilt check
    internal static readonly GuiVertex[] _QuadCache = new GuiVertex[1024];
    
    private static GuiRenderer? renderer;

    private static Stack<Layer> guiLayers = new();
    private static Layer? topLayer {
        get => guiLayers.TryPeek(out var layer) ? layer : null;
    }

    public static void PushLayer(Layer layer) {
       guiLayers.Push(layer);
       topLayer?.InvalidateQuadCache();
    }

    // Remember to drop dangling references to GuiRects in popped layers,
    // otherwise they'll circularly keep the whole tree alive
    public static void PopLayer() {
        guiLayers.Pop();
        topLayer?.InvalidateQuadCache();
    }
    
    public static vec2 ScreenToPixel(vec2 s, vec2 referenceResolution)
        => (s + vec2.Ones) / 2 * referenceResolution;
    
    public static vec2 PixelToScreen(vec2 p, vec2 referenceResolution)
        => p / referenceResolution * 2 - vec2.Ones;
    
    public static void Init(GuiRenderer renderer) {
        GuiCanvas.renderer = renderer;
        ReferenceResolution = new(renderer.Client.NativeWindow.Width, renderer.Client.NativeWindow.Height);
    }

    internal static Atlas.Sprite? GetSprite(string spriteName) {
        if (renderer?.GuiAtlas.TryGetSprite($"gui/{spriteName}", out var sprite) == true) {
            return sprite;
        } else {
            Game.Logger.Warn($"GUI sprite {spriteName} does not exist");
            return null;
        }
    }

    internal static Atlas.Sprite? GetSprite(ResourceKey spriteName) {
        if (renderer?.GuiAtlas.TryGetSprite(spriteName, out var sprite) == true) {
            return sprite;
        } else {
            Game.Logger.Warn($"GUI sprite {spriteName} does not exist");
            return null;
        }
    }

    public class Layer {
        // The root of the GUI tree
        public GuiRect root;
        
        internal bool quadCacheNeedsRebuilt = true;
        
        /// <summary>
        /// A collection of GuiRects whose children need rebuilt, sorted by tree depth.
        /// </summary>
        internal SortedSet<GuiRect> branchesToRebuild = new(new GuiRect.ByTreeDepth());
        
        public Layer() {
            root = new(this);
        }
        
        public void InvalidateQuadCache()
            => quadCacheNeedsRebuilt = true;

        internal void UpdateQuadCache() {
            if (quadCacheNeedsRebuilt) {
                RebuildQuadCache();
                quadCacheNeedsRebuilt = false;
                // resetting this afterwards causes a stack overflow if something in RebuildQuadCache is accessing _QuadCache through the property
            } else while (branchesToRebuild.Count > 0) {
                // nodes in branchesToRebuild are sorted by treeDepth, so lower depth nodes will be rebuilt first
                // this prevents rebuilding deep nodes multiple times as their parents are rebuilt
                var branchParent = branchesToRebuild.Min;
                branchParent!.Rebuild(branchParent!.parent?.globalScreenPosition ?? -vec2.Ones, branchParent!.parent?.globalScreenSize ?? vec2.Ones);
            }
        }
        // Assign each GuiRect a new quadIdx, then rebuild all of them
        internal void RebuildQuadCache() {
            QuadCount = 0;
            branchesToRebuild.Clear();
            root!.Rebuild(-vec2.Ones, vec2.Ones, true);
        }
    }
}
