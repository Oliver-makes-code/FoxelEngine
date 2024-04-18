using GlmSharp;
using Veldrid;

namespace Voxel.Client.Rendering.VertexTypes; 

public struct GuiVertex {

    public static readonly VertexLayoutDescription Layout = new(
        new VertexElementDescription("Position", VertexElementFormat.Float2, VertexElementSemantic.Position),
        new VertexElementDescription("UV", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate)
    );

    public vec2 position;
    public vec2 uv;

    public GuiVertex() {}
    public GuiVertex(vec2 pos) : this(pos, vec2.Zero) {}
    public GuiVertex(vec2 pos, vec2 uv) {
        position = pos;
        this.uv = uv;
    }
}
