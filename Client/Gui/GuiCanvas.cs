using System;
using System.Collections.Generic;
using GlmSharp;
using Voxel.Client.Rendering.Gui;
using Voxel.Client.Rendering.Texture;
using Voxel.Client.Rendering.VertexTypes;
using Voxel.Core;

namespace Voxel.Client.Gui;

// x and y go from -1 to 1, from the bottom left to the bottom right

public static class GuiCanvas {
    // TODO: Update this as the window changes size
    public static ivec2 ReferenceResolution { get; private set; } = ivec2.Zero;

    public static GuiVertex[] QuadCache {
        get {
            topLayer.UpdateQuadCache();
            return _QuadCache;
        }
    }
    public static uint QuadCount { get; internal set; }

    // internal so GuiRect.Rebuild() can bypass the needs rebuilt check
    internal static readonly GuiVertex[] _QuadCache = new GuiVertex[1024];
    
    private static GuiRenderer? renderer;

    private static Stack<Layer> guiLayers = new();
    private static Layer topLayer {
        get => guiLayers.Peek();
    }

    public static void PushLayer(Layer layer) {
       guiLayers.Push(layer);
       topLayer.InvalidateQuadCache();
    }

    // Remember to drop dangling references to GuiRects in popped layers,
    // otherwise they'll circularly keep the whole tree alive
    public static void PopLayer() {
        guiLayers.Pop();
        topLayer.InvalidateQuadCache();
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
            // TODO: Log warning
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
        
        public Layer() {
            root = new GuiRect(this);
        }
    }
}

public class GuiRect {
    public delegate void SizeInitializer(GuiRect parent, GuiRect rect);

    /// <summary>
    /// index into the GuiCanvas._QuadCache
    /// </summary>
    private uint quadIdx = 0;
    /// <summary>
    /// used in GuiCanvas for rebuilding individual branches
    /// </summary>
    private uint treeDepth = 0;
    private GuiCanvas.Layer layer;
    
    public GuiRect? parent = null;
    public List<GuiRect> children = new();
    
    /// <summary>
    /// Takes information from this GuiRect's parent to initialize its size when added to the GUI tree.
    /// </summary>
    private SizeInitializer? sizeInitializer = null;

    // allows for partial pixel resolutions. Note that GuiCanvas.ReferenceResolution must still be in whole pixels
    /// <summary>
    /// The pixel resolution of this GuiRect
    /// </summary>
    public vec2 ReferenceResolution => (parent?.ReferenceResolution ?? GuiCanvas.ReferenceResolution) * localScreenSize;
    
    /// <summary>
    /// name of the texture rendered onto this GuiRect<br/>
    /// a GuiRect with an image of "" will not be rendered<br/>
    /// setting this updates the uv coordinates of this GuiRect's vertices.
    /// </summary>
    public string image {
        get => _image;
        set {
            _image = value;
            UpdateVertexUVs();
        }

    }
    private string _image = "";
    
    // Screen members are used for pixel member calculations along with ReferenceResolution

    /// <summary>
    /// x and y are both bound between -1 and 1.<br/>
    /// The anchor is a point inside (or on the edge of) a GuiRect;
    /// it defines how the width and height of the rect make it expand,
    /// and is what gets moved to the GuiRect's position
    /// </summary>
    public vec2 screenAnchor {
        get => _screenAnchor;
        set {
            layer?.branchesToRebuild.Add(this);
            _screenAnchor = value;
        }
    }
    private vec2 _screenAnchor;

    /// <summary>
    /// x and y can be any real number<br/>
    /// The position of this GuiRect in its parent GuiRect.
    /// The origin is the bottom left corner of the parent.
    /// </summary>
    public vec2 localScreenPosition {
        get => _localScreenPosition;
        set {
            layer?.branchesToRebuild.Add(this);
            _localScreenPosition = value;
        }
    }
    private vec2 _localScreenPosition;
    
    // reading and setting this is expensive, and caching it is impractical for now.
    public vec2 globalScreenPosition {
        get {
            if (parent == null)
                return localScreenPosition;
                
            var localPos = localScreenPosition + vec2.Ones;
            localPos /= 2;

            return parent.globalScreenPosition + localPos * parent.globalScreenSize;
        }
        set {
            var globalDelta = value - globalScreenPosition;
            localScreenPosition += globalDelta / parent?.globalScreenSize ?? vec2.Ones;
        }
    }

    /// <summary>
    /// x and y can be any real number<br/>
    /// The width and height of the GuiRect, from the anchor. ex:<br/>
    /// 1 is the width of the entire parent GuiRect
    /// if the anchor's x position is 0.5, and the width is 12,
    /// the rect will extend 9 units left and 3 units right from the anchor
    /// </summary>
    public vec2 localScreenSize {
        get => _localScreenSize;
        set {
            layer?.branchesToRebuild.Add(this);
            _localScreenSize = value;
        }
    }
    private vec2 _localScreenSize;
    
    // reading and setting this is expensive, and caching it is impractical for now.
    public vec2 globalScreenSize {
        get => localScreenSize * parent?.globalScreenSize ?? vec2.Ones;
        set => localScreenSize = value / parent?.globalScreenSize ?? vec2.Ones;
    }

    /* ----- Pixel members are derived from screen values ----- */
    
    // TODO: The pixel size changes with window resolution. Implement some mechanism for keeping GuiRects at a constant pixel size
    // These dont have a local/global distinction, because they refer to physical size on the monitor
    
    public vec2 pixelPosition {
        get => GuiCanvas.ScreenToPixel(globalScreenPosition, GuiCanvas.ReferenceResolution);
        set => globalScreenPosition = GuiCanvas.PixelToScreen(value, GuiCanvas.ReferenceResolution);
    }

    public vec2 pixelSize {
        get => localScreenSize * (parent?.ReferenceResolution ?? GuiCanvas.ReferenceResolution);
        set => localScreenSize = value / (parent?.ReferenceResolution ?? GuiCanvas.ReferenceResolution);
    }
    
    public Extents extents => new(this);

    
    public GuiRect(vec2 screenAnchor, vec2 localScreenPos, vec2 localScreenSize, string image = "") {
        this.screenAnchor = screenAnchor;
        this.localScreenPosition = localScreenPos;
        this.localScreenSize = localScreenSize;
        this.image = image;
    }

    public GuiRect(vec2 screenAnchor, vec2 localScreenPos, SizeInitializer sizeInitializer, string image = "") {
        this.screenAnchor = screenAnchor;
        this.localScreenPosition = localScreenPos;
        this.sizeInitializer = sizeInitializer;
        this.image = image;
    }

    /// <summary>
    /// Internal constructor that initializes the root of a GUI tree with the proper dimensions
    /// </summary>
    internal GuiRect(GuiCanvas.Layer layer) {
        screenAnchor = new(-1, -1);
        localScreenPosition = new(-1, -1);
        localScreenSize = new(1, 1);
        this.layer = layer;
    }

    /// <returns>
    /// a closure that encapsulates these parameters and will give return a GuiRect with the proper dimensions
    /// </returns>
    public static SizeInitializer FromPixelAspectRatioAndHeight(float ratioWidth, float ratioHeight, float screenHeight)
        => (GuiRect parent, GuiRect rect) => {
            float heightToWidth = ratioWidth / ratioHeight;
            float pixelHeight = parent.ReferenceResolution.y * screenHeight;
            float pixelWidth = heightToWidth * pixelHeight;

            rect.pixelSize = new(pixelWidth, pixelHeight);
        };

    /// <summary>
    /// Adds a child into the GUI tree.
    /// </summary>
    /// <returns>
    /// the added child
    /// </returns>
    public GuiRect AddChild(GuiRect rect) {
        children.Add(rect);
        rect.parent = this;
        rect.treeDepth = treeDepth + 1;
        rect.layer = layer;
        
        layer.InvalidateQuadCache();
        // this needs a complete rebuild to keep indices contiguous when recursively iterating through the gui tree
        // contiguous indices ensure the draw order of GUI elements is correct

        if (rect.sizeInitializer != null)
            rect!.sizeInitializer(this, rect);

        return rect;
    }

    /// <summary>
    /// add this node and all its children to a deletion queue in GuiCanvas
    /// </summary>
    public void Delete()
        => throw new NotImplementedException();

    /// <summary>
    /// Relies on quadIdx being correct, and the GuiVertices in QuadCache having correct screen coordinates
    /// </summary>
    private void UpdateVertexUVs() {
        // GuiRects without images are still included in the QuadCache
        // This just sets their screen position outside of clip space so they'll be discarded
        if (image == "") {
            GuiCanvas._QuadCache[quadIdx + 0] = new(new(-10, -10), vec2.Zero);
            GuiCanvas._QuadCache[quadIdx + 1] = new(new(-10, -10), vec2.Zero);
            GuiCanvas._QuadCache[quadIdx + 2] = new(new(-10, -10), vec2.Zero);
            GuiCanvas._QuadCache[quadIdx + 3] = new(new(-10, -10), vec2.Zero);
            return;
        }
        
        var sprite = GuiCanvas.GetSprite(image); // TODO: Ask Cass how best to cache this
        
        // Null-coalescing for when the image is set during initialization
        var uvTopLeft = sprite?.uvPosition ?? vec2.Zero;
        var uvBottomRight = uvTopLeft + (sprite?.uvSize ?? vec2.Zero);
        var uvBottomLeft = new vec2(uvTopLeft.x, uvBottomRight.y);
        var uvTopRight = new vec2(uvBottomRight.x, uvTopLeft.y);
        
        GuiCanvas._QuadCache[quadIdx + 0] = new(GuiCanvas._QuadCache[quadIdx + 0].position, uvBottomRight);
        GuiCanvas._QuadCache[quadIdx + 1] = new(GuiCanvas._QuadCache[quadIdx + 1].position, uvBottomLeft);
        GuiCanvas._QuadCache[quadIdx + 2] = new(GuiCanvas._QuadCache[quadIdx + 2].position, uvTopLeft);
        GuiCanvas._QuadCache[quadIdx + 3] = new(GuiCanvas._QuadCache[quadIdx + 3].position, uvTopRight);
    }
    
    /// <summary>
    /// Completely rebuilds this node of the GUI tree and all of its children
    /// </summary>
    internal void Rebuild(vec2 globalParentBottomLeftPosition, vec2 globalParentSize, bool rebuildingEntireQuadCache = false) {
        if (rebuildingEntireQuadCache)
            quadIdx = GuiCanvas.QuadCount++ * 4;
        else // we're in the middle of a partial rebuild
            layer.branchesToRebuild.Remove(this);
        
        var globalSize = localScreenSize * globalParentSize;
        var globalPos = globalParentBottomLeftPosition + (localScreenPosition + 1) * globalParentSize; // TODO: Add rotation
        
        var e = new Extents(screenAnchor, globalPos, globalSize);
    
        GuiCanvas._QuadCache[quadIdx + 0] = new(e.ScreenBottomRight, GuiCanvas._QuadCache[quadIdx + 0].uv);
        GuiCanvas._QuadCache[quadIdx + 1] = new(e.ScreenBottomLeft, GuiCanvas._QuadCache[quadIdx + 1].uv);
        GuiCanvas._QuadCache[quadIdx + 2] = new(e.ScreenTopLeft, GuiCanvas._QuadCache[quadIdx + 2].uv);
        GuiCanvas._QuadCache[quadIdx + 3] = new(e.ScreenTopRight, GuiCanvas._QuadCache[quadIdx + 3].uv);
        
        // After the quadIdx has been set and the vertices have been set up
        if (rebuildingEntireQuadCache)
            UpdateVertexUVs();
        
        foreach (var c in children)
            c.Rebuild(e.ScreenBottomLeft, globalSize, rebuildingEntireQuadCache);
    }
    
    public struct Extents {
        public readonly vec2 ScreenBottomLeft;
        public readonly vec2 ScreenTopRight;
        public vec2 ScreenBottomRight => new(ScreenTopRight.x, ScreenBottomLeft.y);
        public vec2 ScreenTopLeft => new(ScreenBottomLeft.x, ScreenTopRight.y);

        public vec2 PixelTopLeft => GuiCanvas.ScreenToPixel(ScreenTopLeft, GuiCanvas.ReferenceResolution);
        public vec2 PixelBottomRight => GuiCanvas.ScreenToPixel(ScreenBottomRight, GuiCanvas.ReferenceResolution);
        public vec2 PixelTopRight => GuiCanvas.ScreenToPixel(ScreenTopRight, GuiCanvas.ReferenceResolution);
        public vec2 PixelBottomLeft => GuiCanvas.ScreenToPixel(ScreenBottomLeft, GuiCanvas.ReferenceResolution);

        public Extents(GuiRect rect) {
            if (
                rect.screenAnchor.x < -1 ||
                rect.screenAnchor.x > 1 ||
                rect.screenAnchor.y < -1 ||
                rect.screenAnchor.y > 1
            ) {
                // TODO: Warn that anchor is out of range
            }

            // The percentage of each dimensions that goes in the negative direction
            // == (0.5, 0.5) if anchor is at (0, 0)
            // https://www.desmos.com/calculator/mpfe8d8fhv
            var percentNegative = rect.screenAnchor / 2 + new vec2(0.5f, 0.5f);
            var percentPositive = 1 - percentNegative;

            var globalPos = rect.globalScreenPosition;
            var globalSize = rect.globalScreenSize;
            
            globalPos += vec2.Ones;
            globalPos /= 2;
            
            ScreenBottomLeft = globalPos - percentNegative * globalSize;
            ScreenTopRight = globalPos + percentPositive * globalSize;

            ScreenBottomLeft *= 2;
            ScreenBottomLeft -= vec2.Ones;
            ScreenTopRight *= 2;
            ScreenTopRight -= vec2.Ones;
        }

        // For quad building, avoids expensive recursive calls
        internal Extents(vec2 anchor, vec2 globalPos, vec2 globalSize) {

            // The percentage of each dimensions that goes in the negative direction
            // == (0.5, 0.5) if anchor is at (0, 0)
            // https://www.desmos.com/calculator/mpfe8d8fhv
            var percentNegative = anchor / 2 + new vec2(0.5f, 0.5f);
            var percentPositive = 1 - percentNegative;

            globalPos += vec2.Ones;
            globalPos /= 2;
            
            ScreenBottomLeft = globalPos - percentNegative * globalSize;
            ScreenTopRight = globalPos + percentPositive * globalSize;

            ScreenBottomLeft *= 2;
            ScreenBottomLeft -= vec2.Ones;
            ScreenTopRight *= 2;
            ScreenTopRight -= vec2.Ones;
        }
    }

    public class ByTreeDepth : IComparer<GuiRect> {
        public int Compare(GuiRect? lhs, GuiRect? rhs)
            => (int)(lhs!.treeDepth - rhs!.treeDepth);
    }
}
