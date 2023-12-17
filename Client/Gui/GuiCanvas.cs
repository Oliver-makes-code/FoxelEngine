// TODO: Make all this recursive somehow, so GuiCanvas and GuiRect implement a GuiParent interface or something

using System;
using System.Collections.Generic;
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
    public static List<GuiRect> elements = new List<GuiRect>();
    
    public static void Init(GuiRenderer renderer) {
        GuiCanvas.renderer = renderer;
        ReferenceResolution = new(renderer.Client.NativeWindow.Width, renderer.Client.NativeWindow.Height);
        
        //RegisterRect(new GuiRect(new(-1, -1), new(-1, -1), new(128, 128)));
        //RegisterRect(new GuiRect(new( 0, -1), new( 0, -1), new(128, 128)));
        //RegisterRect(new GuiRect(new( 1, -1), new( 1, -1), new(128, 128)));
        //RegisterRect(new GuiRect(new(-1,  0), new(-1,  0), new(128, 128)));
        //RegisterRect(new GuiRect(new( 0,  0), new( 0,  0), new(128, 128)));
        //RegisterRect(new GuiRect(new( 1,  0), new( 1,  0), new(128, 128)));
        //RegisterRect(new GuiRect(new(-1,  1), new(-1,  1), new(128, 128)));
        //RegisterRect(new GuiRect(new( 0,  1), new( 0,  1), new(128, 128)));
        //RegisterRect(new GuiRect(new( 1,  1), new( 1,  1), new(128, 128)));
    }
    public static void RegisterRect(GuiRect rect) {
        elements.Add(rect);
    }

    public static GuiVertex[] GetVertices() {
        GuiVertex[] verts = new GuiVertex[elements.Count * 4];
        uint vIdx = 0;
        foreach (var e in elements) {
            var extents = e.extents;
            verts[vIdx++] = new( extents.ScreenTopRight,    new(1, 0) );
            verts[vIdx++] = new( extents.ScreenTopLeft,     new(0, 0) );
            verts[vIdx++] = new( extents.ScreenBottomLeft,  new(0, 1) );
            verts[vIdx++] = new( extents.ScreenBottomRight, new(1, 1) );
        }

        return verts;
    } 
}

public class GuiRect {

    public struct Extents {
        public Extents(GuiRect rect) {
            if (rect.screenAnchor.x < -1 || rect.screenAnchor.x > 1 || rect.screenAnchor.y < -1 || rect.screenAnchor.y > 1) {  
                // TODO: Warn that anchor is out of range
            }
            
            // The percentage of each dimensions that goes in the negative direction
            // == (0.5, 0.5) if anchor is at (0, 0)
            // https://www.desmos.com/calculator/mpfe8d8fhv
            vec2 percentNegative = rect.screenAnchor / 2 + new vec2(0.5f, 0.5f);
            vec2 percentPositive = 1 - percentNegative;

            ScreenTopLeft = rect.screenPosition - percentNegative * rect.screenSize;
            ScreenBottomRight = rect.screenPosition + percentPositive * rect.screenSize;
        }
        
        public readonly vec2 ScreenTopLeft;
        public readonly vec2 ScreenBottomRight;
        public vec2 ScreenTopRight   { get => new vec2(ScreenBottomRight.x, ScreenTopLeft.y); }
        public vec2 ScreenBottomLeft { get => new vec2(ScreenTopLeft.x, ScreenBottomRight.y); }
        
        public vec2 PixelTopLeft     { get => GuiCanvas.ScreenToPixel(ScreenTopLeft);     }
        public vec2 PixelBottomRight { get => GuiCanvas.ScreenToPixel(ScreenBottomRight); }
        public vec2 PixelTopRight    { get => GuiCanvas.ScreenToPixel(ScreenTopRight);    }
        public vec2 PixelBottomLeft  { get => GuiCanvas.ScreenToPixel(ScreenBottomLeft);  }
    }

    public GuiRect(vec2 screenAnchor, vec2 screenPosition, vec2 pixelSize) {
        this.screenAnchor = screenAnchor;
        this.screenPosition = screenPosition;
        this.pixelSize = pixelSize;
    }
    
    // Screen members are used for calculations along with GuiCanvas.ReferenceResolution
    
    // x and y are both bound between -1 and 1.
    // The anchor is a point inside (or on the edge of) a GuiRect;
    // it defines how the width and height of the rect make it expand,
    // and is what gets moved to the GuiRect's position
    public vec2 screenAnchor;
    
    // x and y can be any real number
    // The position of the GuiRect on the screen
    public vec2 screenPosition;
    
    // x and y can be any real number
    // The width and height of the GuiRect, from the anchor.
    // if the anchor's x position is 0.5, and the width is 12,
    // the rect will extend 9 units left and 3 units right from the anchor
    public vec2 screenSize;
    
    // TODO: These only work if the resolution remains constant
    public vec2 pixelAnchor {
        get => GuiCanvas.ScreenToPixel(screenAnchor);
        set => screenAnchor = GuiCanvas.PixelToScreen(value);
    }
    public vec2 pixelPosition {
        get => GuiCanvas.ScreenToPixel(screenPosition);
        set => screenPosition = GuiCanvas.PixelToScreen(value);
    }
    public vec2 pixelSize {
        get => screenSize * GuiCanvas.ReferenceResolution;
        set => screenSize = value / GuiCanvas.ReferenceResolution;
    }
    
    public Extents extents { get => new Extents(this); }
}
