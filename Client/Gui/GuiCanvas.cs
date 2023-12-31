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
    public static vec2 ScreenToPixel(vec2 s)
        => (s + vec2.Ones) / 2 * ReferenceResolution;
    public static vec2 PixelToScreen(vec2 p)
        => p / ReferenceResolution * 2 - vec2.Ones;

    private static GuiRenderer? renderer = null;
    // The root of the GUI tree
    public static GuiRect screen;
    
    public static void Init(GuiRenderer renderer) {
        GuiCanvas.renderer = renderer;
        ReferenceResolution = new(renderer.Client.NativeWindow.Width, renderer.Client.NativeWindow.Height);
        screen = new GuiRect(vec2.Zero, vec2.Zero, ReferenceResolution);
        
        screen.AddChild(new GuiRect(new(-1, -1), new(-1, -1), new(0.1f, 0.178f)));
        screen.AddChild(new GuiRect(new( 0, -1), new( 0, -1), new(0.1f, 0.178f)));
        screen.AddChild(new GuiRect(new( 1, -1), new( 1, -1), new(0.1f, 0.178f)));
        screen.AddChild(new GuiRect(new(-1,  0), new(-1,  0), new(0.1f, 0.178f)));
        screen.AddChild(new GuiRect(new( 0,  0), new( 0,  0), new(0.1f, 0.178f)));
        screen.AddChild(new GuiRect(new( 1,  0), new( 1,  0), new(0.1f, 0.178f)));
        screen.AddChild(new GuiRect(new(-1,  1), new(-1,  1), new(0.1f, 0.178f)));
        screen.AddChild(new GuiRect(new( 0,  1), new( 0,  1), new(0.1f, 0.178f)));
        screen.AddChild(new GuiRect(new( 1,  1), new( 1,  1), new(0.1f, 0.178f)));
        
        foreach(var g in screen.children)
        {
            g.AddChild(new GuiRect(new(-1, -1), new(-1, -1), new(0.1f, 0.178f)));
            g.AddChild(new GuiRect(new( 0, -1), new( 0, -1), new(0.1f, 0.178f)));
            g.AddChild(new GuiRect(new( 1, -1), new( 1, -1), new(0.1f, 0.178f)));
            g.AddChild(new GuiRect(new(-1,  0), new(-1,  0), new(0.1f, 0.178f)));
            g.AddChild(new GuiRect(new( 0,  0), new( 0,  0), new(0.1f, 0.178f)));
            g.AddChild(new GuiRect(new( 1,  0), new( 1,  0), new(0.1f, 0.178f)));
            g.AddChild(new GuiRect(new(-1,  1), new(-1,  1), new(0.1f, 0.178f)));
            g.AddChild(new GuiRect(new( 0,  1), new( 0,  1), new(0.1f, 0.178f)));
            g.AddChild(new GuiRect(new( 1,  1), new( 1,  1), new(0.1f, 0.178f)));
        }
    }

    public static GuiVertex[] QuadCache { get; private set; } = new GuiVertex[1024];
    public static uint QuadCount { get; private set; } = 0;
    
    // Assign each GuiRect a new quadIdx, then rebuild all of them
    private static void RebuildQuadCache() {
        
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

            ScreenTopLeft = rect.localScreenPosition - percentNegative * rect.localScreenSize * 2;
            ScreenBottomRight = rect.localScreenPosition + percentPositive * rect.localScreenSize * 2;
            // multiply by 2 to avoid converting screenPosition from [-1, 1] to [0, 1] and back
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
            get => GuiCanvas.ScreenToPixel(ScreenTopLeft);
        }
        public vec2 PixelBottomRight {
            get => GuiCanvas.ScreenToPixel(ScreenBottomRight);
        }
        public vec2 PixelTopRight {
            get => GuiCanvas.ScreenToPixel(ScreenTopRight);
        }
        public vec2 PixelBottomLeft {
            get => GuiCanvas.ScreenToPixel(ScreenBottomLeft);
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
    }

    // add to a deletion queue in GuiCanvas
    public void Delete() {
        throw new NotImplementedException();
    }

    // allows for partial pixel resolutions. Note that GuiCanvas.ReferenceResolution must still be in whole pixels
    public vec2 ReferenceResolution {
        get {
            if (parent == null) {
                return GuiCanvas.ReferenceResolution * localScreenSize;
            } else {
                return parent!.ReferenceResolution * localScreenSize;
            }
        }
    }

    // Screen members are used for calculations along with ReferenceResolution

    // x and y are both bound between -1 and 1.
    // The anchor is a point inside (or on the edge of) a GuiRect;
    // it defines how the width and height of the rect make it expand,
    // and is what gets moved to the GuiRect's position
    public vec2 screenAnchor;

    // x and y can be any real number
    // The position of this GuiRect in its parent GuiRect
    private vec2 _localScreenPosition;
    public vec2 localScreenPosition {
        get => _localScreenPosition;
        set {
            _localScreenPosition = value;
            
            vec2 cachedGlobalPositionOffsetForChildren = localScreenPosition * cachedGlobalSizeMultiplier;
            cachedGlobalPositionOffsetForChildren += parent?.cachedGlobalPositionOffset ?? vec2.Zero;
            foreach (var c in children) {
                c.cachedGlobalPositionOffset = cachedGlobalPositionOffsetForChildren;
            }
        }
    }
    
    public vec2 globalScreenPosition {
        get {
            if (parent == null)
                return localScreenPosition;
                
            var localPos = localScreenPosition + vec2.Ones;
            localPos /= 2;

            return parent!.cachedGlobalPositionOffset + localPos * parent!.cachedGlobalSizeMultiplier;
        }
        set {
            var globalDelta = value - globalScreenPosition;
            localScreenPosition += globalDelta / cachedGlobalSizeMultiplier;
            cachedGlobalPositionOffset += globalDelta;
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
            _localScreenSize = value;
            cachedGlobalSizeMultiplier = parent?.cachedGlobalSizeMultiplier ?? vec2.Ones * localScreenSize;
        }
    };
    
    // Relies upon cachedGlobalSizeMultiplier being correct
    public vec2 globalScreenSize {
        // ex: localScreenSize is 1, parent's global size is 0.1
        // cachedGlobalSizeMultiplier == 0.1
        // local size of 1 * multiplier of 0.1 gives global size of 0.1
        get => localScreenSize * parent?.cachedGlobalSizeMultiplier ?? vec2.Ones;
        // ex: value is 1, parent's global size is 0.1
        // cachedGlobalSizeMultiplier == 0.1
        // value of 1 / multiplier of 0.1 gives a local size of 10, global size of 1
        set => localScreenSize = value / parent?.cachedGlobalSizeMultiplier ?? vec2.Ones;
    }

    // TODO: These only work if the resolution remains constant
    public vec2 pixelAnchor {
        get => GuiCanvas.ScreenToPixel(screenAnchor);
        set => screenAnchor = GuiCanvas.PixelToScreen(value);
    }
    public vec2 localPixelPosition {
        get => GuiCanvas.ScreenToPixel(localScreenPosition);
        set => localScreenPosition = GuiCanvas.PixelToScreen(value);
    }
    public vec2 localPixelSize {
        get => localScreenSize * GuiCanvas.ReferenceResolution;
        set => localScreenSize = value / GuiCanvas.ReferenceResolution;
    }
    
    public Extents extents { get => new Extents(this); }

    // Completely rebuilds this node of the GUI tree and all of its children
    private void Rebuild(uint quadIdx, vec2 globalSizeMultiplier, vec2 globalPositionOffset) {
        
    }
    // multiplier to get the screen size of children from local to global
    private vec2 _cachedGlobalSizeMultiplier;
    private vec2 cachedGlobalSizeMultiplier {
        get => _cachedGlobalSizeMultiplier;
        set {
            _cachedGlobalSizeMultiplier = value;
            foreach(var c in children) {
                c.cachedGlobalSizeMultiplier = cachedGlobalSizeMultiplier * c.localScreenSize;
            }
        }
    }
    private vec2 _cachedGlobalPositionOffset;
    // offset in screen coordinates from [-1, -1] to the position of this GuiRect's parent.
    private vec2 cachedGlobalPositionOffset {
        get => _cachedGlobalPositionOffset();
        set {
            _cachedGlobalPositionOffset = value;
            foreach(var c in children) {
                c.cachedGlobalPositionOffset = cachedGlobalPositionOffset + c.localScreenPosition * cachedGlobalSizeMultiplier;
            }
        }
    }
}
