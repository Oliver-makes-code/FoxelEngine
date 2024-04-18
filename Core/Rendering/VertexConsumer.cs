using System.Runtime.InteropServices;
using GlmSharp;

namespace Voxel.Core.Rendering;

public class VertexConsumer<TVertex> where TVertex : struct, Vertex<TVertex> {
    private readonly List<TVertex> Vertices = [];
    
    public int Count => Vertices.Count;

    public void Clear()
        => Vertices.Clear();
    
    public TVertex[] ToArray()
        => [.. Vertices];
        
    public Span<TVertex> AsSpan()
        => CollectionsMarshal.AsSpan(Vertices);

    public VertexBuilder Vertex()
        => new(this);
    
    public VertexConsumer<TVertex> Vertex(TVertex vertex) {
        Vertices.Add(vertex);
        return this;
    }

    public class VertexBuilder(VertexConsumer<TVertex> consumer) {
        TVertex Vertex = default;

        public VertexBuilder Position(vec3 position) {
            Vertex = Vertex.WithPosition(position);
            return this;
        }
        public VertexBuilder UvMin(vec2 uvMin) {
            Vertex = Vertex.WithUvMin(uvMin);
            return this;
        }
        public VertexBuilder UvMax(vec2 uvMax) {
            Vertex = Vertex.WithUvMax(uvMax);
            return this;
        }

        public VertexBuilder Uv(vec2 uv) {
            Vertex = Vertex.WithUv(uv);
            return this;
        }

        public VertexBuilder AoCoord(vec2 aoCoord) {
            Vertex = Vertex.WithAoCoord(aoCoord);
            return this;
        }
        
        public VertexBuilder Color(vec4 color) {
            Vertex = Vertex.WithColor(color);
            return this;
        }

        public VertexConsumer<TVertex> Build() {
            consumer.Vertices.Add(Vertex);
            return consumer;
        }
    }
}
