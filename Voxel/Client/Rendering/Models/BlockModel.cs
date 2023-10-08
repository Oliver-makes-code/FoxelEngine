using System.Collections.Generic;
using Voxel.Client.Rendering.VertexTypes;

namespace Voxel.Client.Rendering.Models;

public class BlockModel {

    public static readonly BlockModel Default = new Builder()
        //Left
        .AddVertex(0, new(new(0, 0, 0)))
        .AddVertex(0, new(new(0, 0, 1)))
        .AddVertex(0, new(new(0, 1, 1)))
        .AddVertex(0, new(new(0, 1, 0)))

        //Right
        .AddVertex(1, new(new(1, 0, 0)))
        .AddVertex(1, new(new(1, 1, 0)))
        .AddVertex(1, new(new(1, 1, 1)))
        .AddVertex(1, new(new(1, 0, 1)))

        //Bottom
        /*.AddVertex(2, new BasicVertex(new(0, 0, 0)))
        .AddVertex(2, new BasicVertex(new(0, 0, 0)))
        .AddVertex(2, new BasicVertex(new(0, 0, 0)))
        .AddVertex(2, new BasicVertex(new(0, 0, 0)))

        //Top
        .AddVertex(3, new BasicVertex(new(0, 0, 0)))
        .AddVertex(3, new BasicVertex(new(0, 0, 0)))
        .AddVertex(3, new BasicVertex(new(0, 0, 0)))
        .AddVertex(3, new BasicVertex(new(0, 0, 0)))

        //Backward
        .AddVertex(4, new BasicVertex(new(0, 0, 0)))
        .AddVertex(4, new BasicVertex(new(0, 0, 0)))
        .AddVertex(4, new BasicVertex(new(0, 0, 0)))
        .AddVertex(4, new BasicVertex(new(0, 0, 0)))

        //Forward
        .AddVertex(5, new BasicVertex(new(0, 0, 0)))
        .AddVertex(5, new BasicVertex(new(0, 0, 0)))
        .AddVertex(5, new BasicVertex(new(0, 0, 0)))
        .AddVertex(5, new BasicVertex(new(0, 0, 0)))*/
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
