using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using GlmSharp;
using SixLabors.ImageSharp;
using Veldrid.OpenGLBinding;
using Veldrid.Sdl2;
using Voxel.Client.Rendering.Gui;
using Voxel.Client.Rendering.Texture;
using Voxel.Client.Rendering.VertexTypes;

namespace Voxel.Client.Gui;

// x and y go from -1 to 1, from the bottom left to the bottom right

public static class GuiCanvas {
    // TODO: Update this as the window changes size
    public static ivec2 ReferenceResolution { get; private set; } = ivec2.Zero;
    public static vec2 ScreenToPixel(vec2 s, vec2 referenceResolution)
        => (s + vec2.Ones) / 2 * referenceResolution;
    public static vec2 PixelToScreen(vec2 p, vec2 referenceResolution)
        => p / referenceResolution * 2 - vec2.Ones;

    private static GuiRenderer renderer;
    // The root of the GUI tree
    public static GuiRect screen;
    
    public static void Init(GuiRenderer renderer) {
        GuiCanvas.renderer = renderer;
        ReferenceResolution = new(renderer.Client.NativeWindow.Width, renderer.Client.NativeWindow.Height);
        screen = new GuiRect(-vec2.Ones, -vec2.Ones, vec2.Ones);

        var healthbar = screen.AddChild(new GuiRect(new(1, 1), new(1, 1), new vec2(0.8f, 0.1f), "test"));
        for (int i = 0; i < 7; i++) {
            healthbar.AddChild(new(new(1, 0), new(1 - 0.11f * i, 0), GuiRect.FromPixelAspectRatioAndHeight(9, 8, 1), "test"));
        }
    }

    internal static Atlas.Sprite GetSprite(string spriteName) {
        if (renderer.GuiAtlas.TryGetSprite($"gui/{spriteName}", out var sprite)) {
            return sprite;    
        } else {
            // TODO: Log warning
            Console.WriteLine($"GUI sprite {spriteName} does not exist");
            return null;
        }
    }
    
    // internal so GuiRect.Rebuild() can bypass the needs rebuilt check
    internal static readonly GuiVertex[] _QuadCache = new GuiVertex[1024];
    public static GuiVertex[] QuadCache {
        get {
            if (quadCacheNeedsRebuilt) {
                RebuildQuadCache();
                quadCacheNeedsRebuilt = false;
                // resetting this afterwards causes a stack overflow if something in RebuildQuadCache is accessing _QuadCache through the property
            }
            else if (branchesToRebuild.Count > 0) {
                do {
                    // nodes in branchesToRebuild are sorted by treeDepth, so lower depth nodes will be rebuilt first
                    // this prevents rebuilding deep nodes multiple times as their parents are rebuilt
                    var branchParent = branchesToRebuild.Min;
                    branchParent!.Rebuild(branchParent!.parent?.globalScreenPosition ?? -vec2.Ones, branchParent!.parent?.globalScreenSize ?? vec2.Ones);
                } while (branchesToRebuild.Count > 0);
            }
            
            return _QuadCache;
        }
    }
    public static uint QuadCount { get; internal set; }
    
    // Assign each GuiRect a new quadIdx, then rebuild all of them
    internal static void RebuildQuadCache() {
        QuadCount = 0;
        branchesToRebuild.Clear();
        screen!.Rebuild(-vec2.Ones, vec2.Ones, true);
    }

    private static bool quadCacheNeedsRebuilt = true;
    public static void InvalidateQuadCache() {
        quadCacheNeedsRebuilt = true;
    }

    internal static SortedSet<GuiRect> branchesToRebuild = new SortedSet<GuiRect>(new GuiRect.ByTreeDepth());
}

public class GuiRect {

    public struct Extents {
        public Extents(GuiRect rect) {
            if (rect.screenAnchor.x < -1 || rect.screenAnchor.x > 1 || rect.screenAnchor.y < -1 ||
                rect.screenAnchor.y > 1) {
                // TODO: Warn that anchor is out of range
            }

            // The percentage of each dimensions that goes in the negative direction
            // == (0.5, 0.5) if anchor is at (0, 0)
            // https://www.desmos.com/calculator/mpfe8d8fhv
            vec2 percentNegative = rect.screenAnchor / 2 + new vec2(0.5f, 0.5f);
            vec2 percentPositive = 1 - percentNegative;

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
            vec2 percentNegative = anchor / 2 + new vec2(0.5f, 0.5f);
            vec2 percentPositive = 1 - percentNegative;

            globalPos += vec2.Ones;
            globalPos /= 2;
            
            ScreenBottomLeft = globalPos - percentNegative * globalSize;
            ScreenTopRight = globalPos + percentPositive * globalSize;

            ScreenBottomLeft *= 2;
            ScreenBottomLeft -= vec2.Ones;
            ScreenTopRight *= 2;
            ScreenTopRight -= vec2.Ones;
        }

        public readonly vec2 ScreenBottomLeft;
        public readonly vec2 ScreenTopRight;
        public vec2 ScreenBottomRight {
            get => new vec2(ScreenTopRight.x, ScreenBottomLeft.y);
        }
        public vec2 ScreenTopLeft {
            get => new vec2(ScreenBottomLeft.x, ScreenTopRight.y);
        }

        public vec2 PixelTopLeft {
            get => GuiCanvas.ScreenToPixel(ScreenTopLeft, GuiCanvas.ReferenceResolution);
        }
        public vec2 PixelBottomRight {
            get => GuiCanvas.ScreenToPixel(ScreenBottomRight, GuiCanvas.ReferenceResolution);
        }
        public vec2 PixelTopRight {
            get => GuiCanvas.ScreenToPixel(ScreenTopRight, GuiCanvas.ReferenceResolution);
        }
        public vec2 PixelBottomLeft {
            get => GuiCanvas.ScreenToPixel(ScreenBottomLeft, GuiCanvas.ReferenceResolution);
        }
    }
    public class ByTreeDepth : IComparer<GuiRect>
    {
        public int Compare(GuiRect? lhs, GuiRect? rhs)
            => (int)(lhs!.treeDepth - rhs!.treeDepth);
    }

    // modifies rect based on the parent
    public delegate void SizeInitializer(GuiRect parent, GuiRect rect);
    
    public GuiRect(vec2 screenAnchor, vec2 localScreenPosition, vec2 localScreenSize, string image = "") {
        this.screenAnchor = screenAnchor;
        this.localScreenPosition = localScreenPosition;
        this.localScreenSize = localScreenSize;
        this.image = image;
    }
    public GuiRect(vec2 screenAnchor, vec2 localScreenPosition, SizeInitializer sizeInitializer, string image = "") {
        this.screenAnchor = screenAnchor;
        this.localScreenPosition = localScreenPosition;
        this.sizeInitializer = sizeInitializer;
        this.image = image;
    }

    // returns a closure that encapsulates these parameters and will give return a GuiRect with the proper dimensions
    public static SizeInitializer FromPixelAspectRatioAndHeight(float ratioWidth, float ratioHeight, float screenHeightConstraint) {
        return (GuiRect parent, GuiRect rect) => {
            float heightToWidth = ratioWidth / ratioHeight;
            float pixelHeight = parent.ReferenceResolution.y * screenHeightConstraint;
            float pixelWidth = heightToWidth * pixelHeight;

            rect.pixelSize = new(pixelWidth, pixelHeight);
        };
    }
    
    public GuiRect? parent = null;
    public List<GuiRect> children = new();
    private readonly SizeInitializer? sizeInitializer = null;
    
    // returns the added child
    public GuiRect AddChild(GuiRect rect) {
        children.Add(rect);
        rect.parent = this;
        rect.treeDepth = treeDepth + 1;
        
        GuiCanvas.InvalidateQuadCache();
        // this needs a complete rebuild to keep indices contiguous when recursively iterating through the gui tree
        // contiguous indices ensure the draw order of GUI elements is correct

        if (rect.sizeInitializer != null) rect!.sizeInitializer(this, rect);

        return rect;
    }

    // add to a deletion queue in GuiCanvas
    public void Delete() {
        throw new NotImplementedException();
    }

    // allows for partial pixel resolutions. Note that GuiCanvas.ReferenceResolution must still be in whole pixels
    public vec2 ReferenceResolution {
        get => (parent?.ReferenceResolution ?? GuiCanvas.ReferenceResolution) * localScreenSize;
    }

    // name of the texture rendered onto this GuiRect
    // a GuiRect with an image of "" will not be rendered
    // setting this updates the uv coordinates of this GuiRect's vertices
    private string _image = "";
    public string image {
        get => _image;
        set {
            _image = value;
            UpdateVertexUVs();
        }

    }

    // Screen members are used for pixel member calculations along with ReferenceResolution

    // x and y are both bound between -1 and 1.
    // The anchor is a point inside (or on the edge of) a GuiRect;
    // it defines how the width and height of the rect make it expand,
    // and is what gets moved to the GuiRect's position
    private vec2 _screenAnchor;
    public vec2 screenAnchor {
        get => _screenAnchor;
        set {
            GuiCanvas.branchesToRebuild.Add(this);
            _screenAnchor = value;
        }
    }

    // x and y can be any real number
    // The position of this GuiRect in its parent GuiRect
    private vec2 _localScreenPosition;
    public vec2 localScreenPosition {
        get => _localScreenPosition;
        set {
            GuiCanvas.branchesToRebuild.Add(this);
            _localScreenPosition = value;
        }
    }
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

    // x and y can be any real number
    // The width and height of the GuiRect, from the anchor.
    // 1 is the width of the entire parent GuiRect
    // if the anchor's x position is 0.5, and the width is 12,
    // the rect will extend 9 units left and 3 units right from the anchor
    private vec2 _localScreenSize;
    public vec2 localScreenSize {
        get => _localScreenSize;
        set {
            GuiCanvas.branchesToRebuild.Add(this);
            _localScreenSize = value;
        }
    }
    // reading and setting this is expensive, and caching it is impractical for now.
    public vec2 globalScreenSize {
        get => localScreenSize * parent?.globalScreenSize ?? vec2.Ones;
        set => localScreenSize = value / parent?.globalScreenSize ?? vec2.Ones;
    }

    // Pixel members are derived from screen values
    
    // TODO: The pixel size changes with window resolution. Implement some mechanism for keeping GuiRects at a constant pixel size
    // These dont have a local/global distinction, because they refer to physical size on the monitor
    public vec2 pixelPosition {
        get => GuiCanvas.ScreenToPixel(globalScreenPosition, GuiCanvas.ReferenceResolution);
        set => globalScreenPosition = GuiCanvas.PixelToScreen(value, GuiCanvas.ReferenceResolution);
    }
    public vec2 pixelSize {
        get => localScreenSize * ReferenceResolution;
        set => localScreenSize = value / GuiCanvas.ReferenceResolution;
    }
    
    public Extents extents { get => new Extents(this); }

    // Completely rebuilds this node of the GUI tree and all of its children
    internal void Rebuild(vec2 globalParentPosition, vec2 globalParentSize, bool rebuildingEntireQuadCache = false) {
        if (rebuildingEntireQuadCache)
            quadIdx = GuiCanvas.QuadCount++ * 4;
        else // we're in the middle of a partial rebuild
            GuiCanvas.branchesToRebuild.Remove(this);
        
        var globalSize = localScreenSize * globalParentSize;
        var globalPos = globalParentPosition + (localScreenPosition + 1) * globalParentSize; // TODO: Add rotation

        //if (image == "") image = "test"; // TODO: test code, delete later
        
        var e = new Extents(screenAnchor, globalPos, globalSize);
        Console.WriteLine($"Sprite {image} at {e.PixelBottomLeft} to {e.PixelTopRight}");
            
        GuiCanvas._QuadCache[quadIdx + 0] = new GuiVertex(e.ScreenBottomRight);
        GuiCanvas._QuadCache[quadIdx + 1] = new GuiVertex(e.ScreenBottomLeft);
        GuiCanvas._QuadCache[quadIdx + 2] = new GuiVertex(e.ScreenTopLeft);
        GuiCanvas._QuadCache[quadIdx + 3] = new GuiVertex(e.ScreenTopRight);
        
        // After the quadIdx has been set and the vertices have been set up
        if(rebuildingEntireQuadCache) UpdateVertexUVs();
        
        foreach (var c in children) {
            c.Rebuild(globalPos, globalSize, rebuildingEntireQuadCache);
        }
    }

    // Relies on quadIdx being correct, and the GuiVertices in QuadCache having correct screen coordinates
    private void UpdateVertexUVs() {
        // GuiRects without images are still included in the QuadCache
        // This just sets their screen position outside of clip space so they'll be discarded
        if (image == "") {
            GuiCanvas._QuadCache[quadIdx + 0] = new GuiVertex(new vec2(-10, -10), vec2.Zero);
            GuiCanvas._QuadCache[quadIdx + 1] = new GuiVertex(new vec2(-10, -10), vec2.Zero);
            GuiCanvas._QuadCache[quadIdx + 2] = new GuiVertex(new vec2(-10, -10), vec2.Zero);
            GuiCanvas._QuadCache[quadIdx + 3] = new GuiVertex(new vec2(-10, -10), vec2.Zero);
            return;
        }
        
        var sprite = GuiCanvas.GetSprite(image); // TODO: Ask Cass how best to cache this
        var uvTopLeft = sprite.uvPosition;
        var uvBottomRight = uvTopLeft + sprite.uvSize;
        var uvBottomLeft = new vec2(uvTopLeft.x, uvBottomRight.y);
        var uvTopRight = new vec2(uvBottomRight.x, uvTopLeft.y);
        
        GuiCanvas._QuadCache[quadIdx + 0] = new GuiVertex(GuiCanvas._QuadCache[quadIdx + 0].position, uvBottomRight);
        GuiCanvas._QuadCache[quadIdx + 1] = new GuiVertex(GuiCanvas._QuadCache[quadIdx + 1].position, uvBottomLeft );
        GuiCanvas._QuadCache[quadIdx + 2] = new GuiVertex(GuiCanvas._QuadCache[quadIdx + 2].position, uvTopLeft    );
        GuiCanvas._QuadCache[quadIdx + 3] = new GuiVertex(GuiCanvas._QuadCache[quadIdx + 3].position, uvTopRight   );
    }

    private uint quadIdx = 0; // index into the GuiCanvas._QuadCache
    private uint treeDepth = 0; // used in GuiCanvas for rebuilding individual branches
}
