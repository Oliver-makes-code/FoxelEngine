using GlmSharp;
using Veldrid;
using Voxel.Client.Rendering.Texture;
using Voxel.Common.Util;

namespace Voxel.Client.Rendering.VertexTypes; 

public class GUIVertex {

    public static readonly VertexLayoutDescription Layout = new(
        new VertexElementDescription("Position", VertexElementFormat.Float2, VertexElementSemantic.Position),
        new VertexElementDescription("UV", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate)
    );

    public vec2 position;
    public vec2 uv;

    public GUIVertex() {}
    public GUIVertex(vec2 pos) : this(pos, vec2.Zero) {}
    public GUIVertex(vec2 pos, vec2 uv) {
        position = pos;
        this.uv = uv;
    }
}
