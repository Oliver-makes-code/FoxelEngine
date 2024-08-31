using System.Runtime.InteropServices;
using GlmSharp;

namespace Foxel.Core.Rendering;

public class VertexConsumer<TVertex> where TVertex : unmanaged, Vertex<TVertex> {
    private readonly List<TVertex> Vertices = [];
    
    public int Count => Vertices.Count;

    public void Clear()
        => Vertices.Clear();
    
    public TVertex[] ToArray()
        => [.. Vertices];
        
    public Span<TVertex> AsSpan()
        => CollectionsMarshal.AsSpan(Vertices);
    
    public VertexConsumer<TVertex> Vertex(TVertex vertex) {
        Vertices.Add(vertex);
        return this;
    }
}
