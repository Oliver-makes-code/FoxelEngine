using System;
using System.Threading.Tasks;
using GlmSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;
using Voxel.Client.Rendering.Models;
using Voxel.Client.Rendering.Utils;
using Voxel.Client.Rendering.VertexTypes;
using Voxel.Core.Assets;
using Voxel.Core.Rendering;

namespace Voxel.Client.Rendering.Texture;

/// <summary>
/// Turns a model into a texture, used primarily for generating GUI textures.
/// </summary>
public class ModelTextureizer {
    private const uint Width = 32*3;
    private const uint Height = 32*3;

    public readonly RenderSystem RenderSystem;

    public readonly VoxelClient Client;

    public readonly PackManager.ReloadTask ReloadTask;

    private readonly TypedVertexBuffer<TerrainVertex> ModelBuffer;
    private readonly VertexConsumer<TerrainVertex> Vertices = new();
    private readonly Veldrid.Texture ColorTexture;
    private readonly Veldrid.Texture DepthTexture;
    private readonly Veldrid.Texture ColorStaging;
    private readonly Veldrid.Texture DepthStaging;
    private readonly Framebuffer Framebuffer;
    private readonly TypedDeviceBuffer<CameraStateManager.CameraData> CameraBuffer;
    private readonly ResourceSet CameraResourceSet;

    public bool shouldSave = false;
    private Pipeline? pipeline;

    public ModelTextureizer(VoxelClient client) {
        Client = client;
        RenderSystem = Client.renderSystem!;
        ModelBuffer = new(RenderSystem.ResourceFactory);
        CameraBuffer = new(
            new() {
                Usage = BufferUsage.UniformBuffer | BufferUsage.Dynamic
            },
            RenderSystem
        ) {
            value = new() {
                viewProjectionMatrix = mat4.Ortho(-4.5f, 4.5f, -4.5f, 4.5f)
            }
        };

        CameraResourceSet = RenderSystem.ResourceFactory.CreateResourceSet(new(
            Client.gameRenderer!.CameraStateManager.CameraResourceLayout,
            CameraBuffer.BackingBuffer
        ));

        ColorTexture = RenderSystem.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
            Width, Height,
            1, 1,
            PixelFormat.R32_G32_B32_A32_Float,
            TextureUsage.RenderTarget
        ));

        DepthTexture = RenderSystem.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
            Width, Height,
            1, 1,
            PixelFormat.R32_Float,
            TextureUsage.DepthStencil
        ));

        ColorStaging = RenderSystem.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
            Width, Height,
            1, 1,
            PixelFormat.R32_G32_B32_A32_Float,
            TextureUsage.Staging
        ));

        DepthStaging = RenderSystem.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
            Width, Height,
            1, 1,
            PixelFormat.R32_Float,
            TextureUsage.Staging
        ));

        Framebuffer = RenderSystem.ResourceFactory.CreateFramebuffer(new FramebufferDescription {
            ColorTargets = [
                new FramebufferAttachmentDescription {
                    Target = ColorTexture
                }
            ],
            DepthTarget = new FramebufferAttachmentDescription {
                Target = DepthTexture
            }
        });

        ReloadTask = PackManager.RegisterResourceLoader(AssetType.Assets, Reload);
    }

    public void SaveTexture() {
        var mappedImage = RenderSystem.GraphicsDevice.Map<RgbaVector>(ColorStaging, MapMode.Read);
        var arr = new RgbaVector[mappedImage.Count];
        for (int i = 0; i < mappedImage.Count; i++)
            arr[i] = mappedImage[i];
        var image = Image.LoadPixelData<RgbaVector>(arr.AsSpan(), (int)Width, (int)Height);
        image.SaveAsPng("color.png");

        var depth = RenderSystem.GraphicsDevice.Map<float>(DepthStaging, MapMode.Read);
        for (int i = 0; i < depth.Count; i++)
            arr[i] = new(depth[i], depth[i], depth[i], 1);
        image = Image.LoadPixelData<RgbaVector>(arr.AsSpan(), (int)Width, (int)Height);
        image.SaveAsPng("depth.png");

        shouldSave = false;
    }

    public void Textureize(BlockModel model) {
        Vertices.Clear();
        foreach (var side in model.SidedVertices)
            foreach (var vtx in side)
                Vertices.Vertex(vtx);
        ModelBuffer.Update(Vertices, RenderSystem.MainCommandList, RenderSystem.ResourceFactory);

        Render();
    }

    private void Render() {
        var commandList = RenderSystem.MainCommandList;

        commandList.SetFramebuffer(Framebuffer);
        commandList.SetPipeline(pipeline);

        commandList.ClearColorTarget(0, new RgbaFloat(0, 0, 0, 0));
        commandList.ClearDepthStencil(1);

        commandList.SetVertexBuffer(0, ModelBuffer.buffer);

        commandList.SetIndexBuffer(RenderSystem.CommonIndexBuffer, IndexFormat.UInt32);

        commandList.SetGraphicsResourceSet(0, CameraResourceSet);

        commandList.DrawIndexed(ModelBuffer.size);
        commandList.CopyTexture(ColorTexture, ColorStaging);
        commandList.CopyTexture(DepthTexture, DepthStaging);

        shouldSave = true;
    }

    private async Task Reload(PackManager packs) {
        await RenderSystem.ShaderManager.ReloadTask;
        await BlockModelManager.ReloadTask;
        RebuildPipeline();
    }

    private void RebuildPipeline() {
        {
            pipeline?.Dispose();
            pipeline = null;
        }
        if (!RenderSystem.ShaderManager.GetShaders(new("shaders/textureizer"), out var shaders))
            throw new("Shaders not present.");
        
        pipeline = RenderSystem.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription {
            Outputs = Framebuffer.OutputDescription,
            BlendState = BlendStateDescription.SingleOverrideBlend,
            DepthStencilState = new() {
                DepthComparison = ComparisonKind.LessEqual,
                DepthTestEnabled = true,
                DepthWriteEnabled = true,
            },
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            RasterizerState = new() {
                CullMode = FaceCullMode.None,
                DepthClipEnabled = true,
                FillMode = PolygonFillMode.Solid,
                FrontFace = FrontFace.CounterClockwise,
                ScissorTestEnabled = false
            },
            ShaderSet = new() {
                VertexLayouts = [
                    PositionVertex.Layout,
                    TerrainVertex.Layout
                ],
                Shaders = shaders
            },
            ResourceLayouts = [
                Client.gameRenderer!.CameraStateManager.CameraResourceLayout
            ]
        });
    }
}
