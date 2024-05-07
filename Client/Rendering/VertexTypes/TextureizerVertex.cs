using GlmSharp;
using Veldrid;
using Voxel.Core.Rendering;

namespace Voxel.Client.Rendering.VertexTypes;

public struct TextureizerVertex : Vertex<TextureizerVertex> {
    public static VertexLayoutDescription Layout { get; } = new(
        new VertexElementDescription("Position", VertexElementFormat.Float3, VertexElementSemantic.Position),
        new VertexElementDescription("Rotation", VertexElementFormat.Float4, VertexElementSemantic.Position)
    ) {
        InstanceStepRate = 1
    };

    public vec3 position;
    public vec4 rotation;

    public TextureizerVertex(vec3 position, quat rotation) {
        this.position = position;
        this.rotation = new(rotation.x, rotation.y, rotation.z, rotation.w);
    }
}
