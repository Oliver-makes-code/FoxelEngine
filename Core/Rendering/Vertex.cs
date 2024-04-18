using GlmSharp;

namespace Voxel.Core.Rendering;

public interface Vertex<TSelf> where TSelf : struct, Vertex<TSelf> {
    public TSelf WithPosition(vec3 position);
    public TSelf WithUv(vec2 uv);
    public TSelf WithUvMin(vec2 uvMin);
    public TSelf WithUvMax(vec2 uvMax);
    public TSelf WithAoCoord(vec2 ao);
    public TSelf WithColor(vec4 color);
}
