using Foxel.Common.World.Content.Noise;
using Foxel.Core.Util;

namespace Foxel.Common.World.Content;


public class NoiseMapContentManager() : ServerContentManager<NoiseMap, NoiseMap>(NoiseMap.Codec, ContentStores.NoiseMaps) {
    public override string ContentDir()
        => "worldgen/noise_maps";
    
    public override NoiseMap Load(ResourceKey key, NoiseMap json)
        => json;
}
