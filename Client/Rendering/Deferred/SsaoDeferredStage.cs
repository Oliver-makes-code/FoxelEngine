using Foxel.Core.Util;
using Veldrid;

namespace Foxel.Client.Rendering.Deferred;

public class SsaoDeferredStage1 : DeferredStage {
    public SsaoDeferredStage1(VoxelClient client, DeferredRenderer parent) : base(client, parent, 0.5f, PixelFormat.R16_G16_B16_A16_Float) {
        
    }

    public override ResourceLayout[] Layouts()
        => [];

    public override ResourceKey ShaderKey()
        => new("shaders/deferred/ssao");
}
