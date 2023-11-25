using System.Collections.Generic;
using Voxel.Client.Rendering.VertexTypes;

namespace Voxel.Client.Rendering.Models;

public class BlockModel {
    public readonly BasicVertex[][] SidedVertices = new BasicVertex[7][];

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
