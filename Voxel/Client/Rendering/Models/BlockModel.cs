using System.Collections.Generic;
using Voxel.Client.Rendering.VertexTypes;

namespace Voxel.Client.Rendering.Models;

public class BlockModel {

    public static readonly BlockModel DEFAULT;


    public BasicVertex[][] SidedVertices = new BasicVertex[7][];

    static BlockModel() {

        DEFAULT = new Builder()
            //Left
            .AddVertex(0, new BasicVertex(new(0, 0, 0)))
            .AddVertex(0, new BasicVertex(new(0, 0, 1)))
            .AddVertex(0, new BasicVertex(new(0, 1, 1)))
            .AddVertex(0, new BasicVertex(new(0, 1, 0)))

            //Right
            .AddVertex(1, new BasicVertex(new(1, 0, 0)))
            .AddVertex(1, new BasicVertex(new(1, 1, 0)))
            .AddVertex(1, new BasicVertex(new(1, 1, 1)))
            .AddVertex(1, new BasicVertex(new(1, 0, 1)))

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
    }


    public class Builder {
        private List<BasicVertex>[] _currentSideCache = new List<BasicVertex>[7];

        public Builder() {
            for (var i = 0; i < _currentSideCache.Length; i++)
                _currentSideCache[i] = new List<BasicVertex>();
        }

        public BlockModel Build() {
            var mdl = new BlockModel();

            for (var i = 0; i < mdl.SidedVertices.Length; i++) {
                var cache = _currentSideCache[i];

                mdl.SidedVertices[i] = cache.ToArray();

                cache.Clear();
            }

            return mdl;
        }

        public Builder AddVertex(uint side, BasicVertex vertex) {
            _currentSideCache[side].Add(vertex);
            return this;
        }
    }
}
