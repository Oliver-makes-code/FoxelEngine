using System.Runtime.InteropServices;
using GlmSharp;
using Veldrid;
using Voxel.Client.Rendering.Utils;
using Voxel.Client.Rendering.VertexTypes;

namespace Voxel.Client.Rendering;

/// <summary>
/// Solely responsible for blitting one texture into another.
/// </summary>
public class BlitRenderer : Renderer {

    private Pipeline DirectPipeline;

    private readonly DeviceBuffer VertexBuffer;

    private readonly TypedDeviceBuffer<BlitParams> Params;
    private readonly ResourceLayout ParamsLayout;
    private readonly ResourceSet ParamsSet;

    public BlitRenderer(VoxelClient client) : base(client) {

        VertexBuffer = RenderSystem.ResourceFactory.CreateBuffer(new BufferDescription {
            SizeInBytes = (uint)Marshal.SizeOf<BasicVertex>() * 4, Usage = BufferUsage.VertexBuffer
        });
        RenderSystem.GraphicsDevice.UpdateBuffer(VertexBuffer, 0, new[] {
            new BasicVertex(new vec3(0, 0, 0)),
            new BasicVertex(new vec3(0, 1, 0)),
            new BasicVertex(new vec3(1, 1, 0)),
            new BasicVertex(new vec3(1, 0, 0)),
        });

        Params = new TypedDeviceBuffer<BlitParams>(new BufferDescription {
            Usage = BufferUsage.UniformBuffer | BufferUsage.Dynamic
        }, RenderSystem);

        ParamsLayout = ResourceFactory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("Params", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment))
        );
        ParamsSet = ResourceFactory.CreateResourceSet(new ResourceSetDescription(
            ParamsLayout,
            Params.BackingBuffer
        ));
    }

    public override void CreatePipeline(MainFramebuffer framebuffer) {

        if (!RenderSystem.ShaderManager.GetShaders("shaders/blit", out var shaders))
            throw new("Blit shaders not found");

        DirectPipeline = framebuffer.AddDependency(ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription {
            Outputs = framebuffer.WindowFramebuffer.OutputDescription,
            BlendState = BlendStateDescription.SingleOverrideBlend,
            DepthStencilState = new DepthStencilStateDescription {
                DepthWriteEnabled = false, DepthTestEnabled = false, StencilTestEnabled = false,
            },
            PrimitiveTopology = PrimitiveTopology.TriangleStrip,
            RasterizerState = new RasterizerStateDescription {
                CullMode = FaceCullMode.None, DepthClipEnabled = false, ScissorTestEnabled = false, FillMode = PolygonFillMode.Solid
            },
            ShaderSet = new ShaderSetDescription {
                VertexLayouts = new[] {
                    BasicVertex.Layout
                },
                Shaders = shaders
            },
            ResourceLayouts = new[] {
                RenderSystem.TextureManager.TextureResourceLayout,
                ParamsLayout,
            }
        }));
    }

    public override void Render(double delta) {

    }

    public override void Dispose() {
        VertexBuffer.Dispose();
    }

    public void Blit(Veldrid.Texture source, Framebuffer destination, bool flip = false) {
        Params.SetValue(new BlitParams {
            flipped = flip
        }, CommandList);

        //Create resource set for this frame
        var set = RenderSystem.TextureManager.CreateTextureResourceSet(source);
        RenderSystem.GraphicsDevice.DisposeWhenIdle(set);

        CommandList.SetPipeline(DirectPipeline);
        CommandList.SetFramebuffer(destination);

        //Set resource sets...
        CommandList.SetGraphicsResourceSet(0, set);
        CommandList.SetGraphicsResourceSet(1, ParamsSet);

        //Finally, draw a quad across the screen.
        CommandList.SetVertexBuffer(0, VertexBuffer);
        CommandList.SetIndexBuffer(RenderSystem.CommonIndexBuffer, IndexFormat.UInt32);
        CommandList.DrawIndexed(6);
    }


    [StructLayout(LayoutKind.Sequential)]
    private struct BlitParams {
        public bool flipped;
    }
}
