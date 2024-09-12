using Veldrid;

namespace Foxel.Core.Rendering;

public interface Vertex<TSelf> where TSelf : unmanaged, Vertex<TSelf> {
    public static abstract VertexLayoutDescription Layout { get; }
}
