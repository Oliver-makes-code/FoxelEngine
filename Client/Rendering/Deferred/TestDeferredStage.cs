using Foxel.Core.Util;
using Veldrid;

namespace Foxel.Client.Rendering.Deferred;

public class TestDeferredStage : DeferredStage {
    public TestDeferredStage(VoxelClient client, DeferredRenderer parent) : base(client, parent, 1, PixelFormat.R16_G16_B16_A16_Float) {
    }

    public override ResourceLayout[] Layouts()
        => [];

    public override ResourceKey ShaderKey()
        => new("shaders/deferred/test");
}
