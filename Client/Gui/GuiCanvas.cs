// TODO: Make all this recursive somehow, so GuiCanvas and GuiRect implement a GuiParent interface or something

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using GlmSharp;
using Veldrid;
using Voxel.Client.Rendering.Gui;
using Voxel.Client.Rendering.VertexTypes;

namespace Voxel.Client.Gui;

// x and y go from -1 to 1, from the top left to the bottom right

public static class GuiCanvas {
    public static ivec2 ReferenceResolution { get; private set; } = ivec2.Zero;
    public static vec2 ScreenToPixel(vec2 s, vec2 referenceResolution)
        => (s + vec2.Ones) / 2 * referenceResolution;
    public static vec2 PixelToScreen(vec2 p, vec2 referenceResolution)
        => p / referenceResolution * 2 - vec2.Ones;

    private static GuiRenderer? renderer = null;
    // The root of the GUI tree
    public static GuiRect screen;
    
    public static void Init(GuiRenderer renderer) {
        GuiCanvas.renderer = renderer;
        ReferenceResolution = new(renderer.Client.NativeWindow.Width, renderer.Client.NativeWindow.Height);
        screen = new GuiRect(-vec2.Ones, -vec2.Ones, vec2.Ones);
        
        screen.AddChild(new(-vec2.Ones, -vec2.Ones, new vec2(128, 128) / ReferenceResolution));
        screen.children[0].AddChild(new(-vec2.Ones, -vec2.Ones, new vec2(0.5f, 0.5f)));
    }

    // internal so GuiRect.Rebuild() can bypass the needs rebuilt check
    internal static readonly GuiVertex[] _QuadCache = new GuiVertex[1024];
    public static GuiVertex[] QuadCache {
        get {
            if (quadCacheNeedsRebuilt) {
                RebuildQuadCache();
                quadCacheNeedsRebuilt = false; // will cause a stack overflow if something in RebuildQuadCache is accessing _QuadCache through the property
            }
            return _QuadCache;
        }
    }
    public static uint QuadCount { get; internal set; }
    
    // Assign each GuiRect a new quadIdx, then rebuild all of them
    internal static void RebuildQuadCache() {
        QuadCount = 0;
        screen.Rebuild(-vec2.Ones, vec2.Ones, true);
    }

    private static bool quadCacheNeedsRebuilt = true;
    public static void InvalidateQuadCache() {
        quadCacheNeedsRebuilt = true;
    }
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
            
            ScreenTopLeft = globalPos - percentNegative * globalSize;
            ScreenBottomRight = globalPos + percentPositive * globalSize;
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
            
            ScreenTopLeft = globalPos - percentNegative * globalSize;
            ScreenBottomRight = globalPos + percentPositive * globalSize;

            ScreenTopLeft *= 2;
            ScreenTopLeft -= vec2.Ones;
            ScreenBottomRight *= 2;
            ScreenBottomRight -= vec2.Ones;
        }

        public readonly vec2 ScreenTopLeft;
        public readonly vec2 ScreenBottomRight;
        public vec2 ScreenTopRight {
            get => new vec2(ScreenBottomRight.x, ScreenTopLeft.y);
        }
        public vec2 ScreenBottomLeft {
            get => new vec2(ScreenTopLeft.x, ScreenBottomRight.y);
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

    public GuiRect(vec2 screenAnchor, vec2 localScreenPosition, vec2 localScreenSize) {
        this.screenAnchor = screenAnchor;
        this.localScreenPosition = localScreenPosition;
        this.localScreenSize = localScreenSize;
    }

    public GuiRect? parent = null;
    public List<GuiRect> children = new();

    public void AddChild(GuiRect rect) {
        children.Add(rect);
        rect.parent = this;
        GuiCanvas.InvalidateQuadCache();
    }

    // add to a deletion queue in GuiCanvas
    public void Delete() {
        throw new NotImplementedException();
    }

    // allows for partial pixel resolutions. Note that GuiCanvas.ReferenceResolution must still be in whole pixels
    public vec2 ReferenceResolution {
        get => (parent?.ReferenceResolution ?? GuiCanvas.ReferenceResolution) * localScreenSize;
    }

    // Screen members are used for calculations along with ReferenceResolution

    // x and y are both bound between -1 and 1.
    // The anchor is a point inside (or on the edge of) a GuiRect;
    // it defines how the width and height of the rect make it expand,
    // and is what gets moved to the GuiRect's position
    public vec2 screenAnchor;

    // x and y can be any real number
    // The position of this GuiRect in its parent GuiRect
    public vec2 localScreenPosition;
    
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
    public vec2 localScreenSize;
    
    // reading and setting this is expensive, and caching it is impractical for now.
    public vec2 globalScreenSize {
        get => localScreenSize * parent?.globalScreenSize ?? vec2.Ones;
        set => localScreenSize = value / parent?.globalScreenSize ?? vec2.Ones;
    }

    // TODO: These only work if the resolution remains constant
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
        
        var globalSize = localScreenSize * globalParentSize;
        var globalPos = globalParentPosition + (localScreenPosition + 1) / 2 * globalParentSize;
        var e = new Extents(screenAnchor, globalPos, globalSize);
        
        GuiCanvas._QuadCache[quadIdx + 0] = new GuiVertex(e.ScreenTopRight    , new(1, 1));
        GuiCanvas._QuadCache[quadIdx + 1] = new GuiVertex(e.ScreenTopLeft     , new(0, 1));
        GuiCanvas._QuadCache[quadIdx + 2] = new GuiVertex(e.ScreenBottomLeft  , new(0, 0));
        GuiCanvas._QuadCache[quadIdx + 3] = new GuiVertex(e.ScreenBottomRight , new(1, 0));
        // I'm pretty sure these UV y coordinates are backwards, but the rects render upside down otherwise
        
        Console.WriteLine(pixelSize);
        
        foreach (var c in children) {
            c.Rebuild(globalPos, globalSize, rebuildingEntireQuadCache);
        }
    }

    private uint quadIdx = 0;
}
