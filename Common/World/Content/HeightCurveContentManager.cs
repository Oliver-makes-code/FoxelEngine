using Foxel.Common.Server;
using Foxel.Common.World.Content.Noise;
using Foxel.Core.Util;

namespace Foxel.Common.World.Content;


public class HeightCurveContentManager() : ServerContentManager<HeightCurve, HeightCurve>(HeightCurve.Codec, ContentStores.HeightCurves) {
    public override string ContentDir()
        => "worldgen/noise/curve";
    
    public override HeightCurve Load(ResourceKey key, HeightCurve json)
        => json;

    public async override Task PreLoad()
        => await VoxelServer.NoiseMapContentManager.ReloadTask;
}
