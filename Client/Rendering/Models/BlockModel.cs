using System.Collections.Generic;
using Foxel.Client.Rendering.VertexTypes;

namespace Foxel.Client.Rendering.Models;

public class BlockModel {
    public readonly TerrainVertex[][] SidedVertices = new TerrainVertex[7][];

    public class Builder {
        private readonly List<TerrainVertex>[] CurrentSideCache = new List<TerrainVertex>[7];

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

        public Builder AddVertex(int side, TerrainVertex vertex) {
            CurrentSideCache[side].Add(vertex);
            return this;
        }

        public Builder AddVertex(CullingSide side, TerrainVertex vertex)
            => AddVertex((int)side, vertex);
    }
}
