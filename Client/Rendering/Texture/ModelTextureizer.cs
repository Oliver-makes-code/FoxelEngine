using System.Threading.Tasks;
using GlmSharp;
using Veldrid;
using Voxel.Client.Rendering.Models;
using Voxel.Client.Rendering.Utils;
using Voxel.Client.Rendering.VertexTypes;
using Voxel.Common.Content;
using Voxel.Core.Assets;
using Voxel.Core.Rendering;

namespace Voxel.Client.Rendering.Texture;

/// <summary>
/// Turns a model into a texture, used primarily for generating GUI textures.
/// </summary>
public class ModelTextureizer {
    public const uint BaseResolution = 32;
    public const uint Tiles = 3;
    public const uint Resolution = BaseResolution * Tiles;
    public const uint Width = Resolution;
    public const uint Height = Resolution;

    /// <summary>
    /// The size of the image in the textureizer
    /// </summary>
    public static readonly ivec2 Size = new((int)Width, (int)Height);

    public readonly RenderSystem RenderSystem;

    public readonly VoxelClient Client;

    /// <summary>
    /// The ReloadTask for the textureizer
    /// </summary>
    public readonly PackManager.ReloadTask ReloadTask;

    /// <summary>
    /// The texture being rendered to
    /// </summary>
    public readonly Veldrid.Texture ColorTexture;

    /// <summary>
    /// The resource set for the texture
    /// </summary>
    public readonly ResourceSet TextureSet;

    /// <summary>
    /// The depth buffer for the textureizer
    /// </summary>
    private readonly Veldrid.Texture DepthTexture;

    /// <summary>
    /// The buffer for the current model
    /// </summary>
    private readonly TypedVertexBuffer<TerrainVertex> ModelBuffer;
    private readonly VertexConsumer<TerrainVertex> Vertices = new();
    private readonly Framebuffer Framebuffer;
    private readonly TypedDeviceBuffer<CameraStateManager.CameraData> CameraBuffer;
    private readonly ResourceSet CameraResourceSet;

    private readonly ResourceLayout ModelTransformLayout;
    private readonly TypedDeviceBuffer<ModelTransformData> ModelTransformBuffer;
    private readonly ResourceSet ModelTransformSet;

    private Pipeline? pipeline;

    public ModelTextureizer(VoxelClient client) {
        Client = client;
        RenderSystem = Client.renderSystem!;

        // Create model transform data
        ModelTransformLayout = RenderSystem.ResourceFactory.CreateResourceLayout(new(
            new ResourceLayoutElementDescription("Model Transform", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)
        ));

        ModelTransformBuffer = new(
            new() {
                Usage = BufferUsage.UniformBuffer | BufferUsage.Dynamic
            },
            RenderSystem
        );

        ModelTransformSet = RenderSystem.ResourceFactory.CreateResourceSet(new(
            ModelTransformLayout,
            ModelTransformBuffer.BackingBuffer
        ));
        
        // Create model buffer
        ModelBuffer = new(RenderSystem.ResourceFactory);

        // Create camera data
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

        // Create textures
        ColorTexture = RenderSystem.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
            Width, Height,
            1, 1,
            PixelFormat.R32_G32_B32_A32_Float,
            TextureUsage.RenderTarget | TextureUsage.Sampled
        ));

        TextureSet = RenderSystem.TextureManager.CreateTextureResourceSet(ColorTexture);

        DepthTexture = RenderSystem.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
            Width, Height,
            1, 1,
            PixelFormat.R32_Float,
            TextureUsage.DepthStencil
        ));

        // Create framebuffer
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

        // Register reloader
        ReloadTask = PackManager.RegisterResourceLoader(AssetType.Assets, Reload);
    }

    /// <summary>
    /// Textureizes a model using a given rotation
    /// </summary>
    /// <param name="model">The model to textureize</param>
    /// <param name="rotation">The rotation of the model</param>
    public void Textureize(BlockModel model, quat rotation) {
        // Copy the model to the buffer
        Vertices.Clear();
        foreach (var side in model.SidedVertices)
            foreach (var vtx in side)
                Vertices.Vertex(vtx);
        ModelBuffer.Update(Vertices, RenderSystem.MainCommandList, RenderSystem.ResourceFactory);

        // Render to the texture
        Render(rotation);
    }

    private void Render(quat rotation) {
        ModelTransformBuffer.value = new(rotation);

        var commandList = RenderSystem.MainCommandList;

        // Set up pipeline and framebuffer
        commandList.SetFramebuffer(Framebuffer);
        commandList.SetPipeline(pipeline);

        // Clear textures
        commandList.ClearColorTarget(0, new RgbaFloat(0, 0, 0, 0));
        commandList.ClearDepthStencil(1);

        // Set buffers
        commandList.SetVertexBuffer(0, ModelBuffer.buffer);

        commandList.SetIndexBuffer(RenderSystem.CommonIndexBuffer, IndexFormat.UInt32);

        // Set reosurce sets
        commandList.SetGraphicsResourceSet(0, CameraResourceSet);
        commandList.SetGraphicsResourceSet(1, Client.gameRenderer!.WorldRenderer.ChunkRenderer.TerrainAtlas.value!.atlasResourceSet);
        commandList.SetGraphicsResourceSet(2, ModelTransformSet);

        // Render
        commandList.DrawIndexed(ModelBuffer.size * 3 / 2);
    }

    private async Task Reload(PackManager packs) {
        // Wait for dependencies
        await RenderSystem.ShaderManager.ReloadTask;

        // Rebuild the pipeline with the new shaders
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
                    TerrainVertex.Layout
                ],
                Shaders = shaders
            },
            ResourceLayouts = [
                Client.gameRenderer!.CameraStateManager.CameraResourceLayout,
                RenderSystem.TextureManager.TextureResourceLayout,
                ModelTransformLayout
            ]
        });
    }

    private struct ModelTransformData {
        public quat rotation;

        public ModelTransformData(quat rotation) {
            this.rotation = rotation;
        }
    }
}