using System.Collections.Generic;
using GlmSharp;
using Voxel.Client.Rendering.VertexTypes;

namespace Voxel.Client.Rendering.Models;

public class BlockModel {

    public static readonly BlockModel Default = new Builder()
        //Left
        .AddVertex(0, new(new(0, 0, 0), vec4.Ones * 0.8f))
        .AddVertex(0, new(new(0, 0, 1), vec4.Ones * 0.8f))
        .AddVertex(0, new(new(0, 1, 1), vec4.Ones * 0.8f))
        .AddVertex(0, new(new(0, 1, 0), vec4.Ones * 0.8f))

        //Right
        .AddVertex(1, new(new(1, 0, 0), vec4.Ones * 0.77f))
        .AddVertex(1, new(new(1, 1, 0), vec4.Ones * 0.77f))
        .AddVertex(1, new(new(1, 1, 1), vec4.Ones * 0.77f))
        .AddVertex(1, new(new(1, 0, 1), vec4.Ones * 0.77f))

        //Bottom
        .AddVertex(2, new(new(0, 0, 0), vec4.Ones * 0.6f))
        .AddVertex(2, new(new(1, 0, 0), vec4.Ones * 0.6f))
        .AddVertex(2, new(new(1, 0, 1), vec4.Ones * 0.6f))
        .AddVertex(2, new(new(0, 0, 1), vec4.Ones * 0.6f))

        //Top
        .AddVertex(3, new(new(0, 1, 0), vec4.Ones))
        .AddVertex(3, new(new(0, 1, 1), vec4.Ones))
        .AddVertex(3, new(new(1, 1, 1), vec4.Ones))
        .AddVertex(3, new(new(1, 1, 0), vec4.Ones))

        //Backward
        .AddVertex(4, new(new(0, 0, 0), vec4.Ones * 0.7f))
        .AddVertex(4, new(new(0, 1, 0), vec4.Ones * 0.7f))
        .AddVertex(4, new(new(1, 1, 0), vec4.Ones * 0.7f))
        .AddVertex(4, new(new(1, 0, 0), vec4.Ones * 0.7f))

        //Forward
        .AddVertex(5, new(new(0, 0, 1), vec4.Ones * 0.67f))
        .AddVertex(5, new(new(1, 0, 1), vec4.Ones * 0.67f))
        .AddVertex(5, new(new(1, 1, 1), vec4.Ones * 0.67f))
        .AddVertex(5, new(new(0, 1, 1), vec4.Ones * 0.67f))
        .Build();


    public BasicVertex[][] SidedVertices = new BasicVertex[7][];


    public class Builder {
        private readonly List<BasicVertex>[] CurrentSideCache = new List<BasicVertex>[7];

        public Builder() {
            for (int i = 0; i < CurrentSideCache.Length; i++)
                CurrentSideCache[i] = new();
        }

        public BlockModel Build() {
            var mdl = new BlockModel();

            for (int i = 0; i < mdl.SidedVertices.Length; i++) {
                var cache = CurrentSideCache[i];

                mdl.SidedVertices[i] = cache.ToArray();

                cache.Clear();
            }

            return mdl;
        }

        public Builder AddVertex(uint side, BasicVertex vertex) {
            CurrentSideCache[side].Add(vertex);
            return this;
        }
    }
}
