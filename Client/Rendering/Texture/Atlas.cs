using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using GlmSharp;
using Veldrid;
using Veldrid.OpenGLBinding;
using Voxel.Client.Rendering.Utils;
using Voxel.Client.Rendering.VertexTypes;
using Voxel.Core;
using Voxel.Core.Rendering;

namespace Voxel.Client.Rendering.Texture;

/// <summary>
/// Represents a single texture atlas.
///
/// The way this works is basically that it stores a single texture that acts as a render target.
/// Whenever we patch a new texture into it, we draw a quad over top of the area with the new texture.
/// </summary>
public class Atlas {

    public const int CellSize = 16;

    public readonly RenderSystem RenderSystem;

    public readonly string Name;

    public CommandList CommandList => RenderSystem.MainCommandList;

    private readonly HashSet<ivec2> availableCellsSet = new();
    private readonly List<ivec2> availableCells = new();

    private readonly Dictionary<string, Sprite> textures = new();

    public ivec2 size => nativeAtlasData.Size;

    private readonly Pipeline DrawPipeline;


    private NativeAtlasData nativeAtlasData;
    public ResourceSet AtlasResourceSet => nativeAtlasData.ResourceSet;

    private readonly ResourceSet TextureDrawParamsResourceSet;
    private readonly TypedDeviceBuffer<TextureDrawUniform> TextureDrawParamsUniform;

    private readonly DeviceBuffer VertexBuffer;

    public Atlas(string name, RenderSystem renderSystem, int cellsHorizontal = 4, int cellsVertical = 4) {
        Name = name;
        RenderSystem = renderSystem;

        if (!RenderSystem.ShaderManager.GetShaders("shaders/stitcher", out var shaders))
            throw new("Blit shaders not found");

        //Native atlas.
        nativeAtlasData = new NativeAtlasData(CellSize * cellsHorizontal, CellSize * cellsVertical, renderSystem);

        for (int x = 0; x < cellsHorizontal; x++)
        for (int y = 0; y < cellsVertical; y++) {
            var cellPos = new ivec2(x * CellSize, y * CellSize);
            UnclaimCell(cellPos);
        }

        //Draw params uniform
        var drawUniformLayout = RenderSystem.ResourceFactory.CreateResourceLayout(new ResourceLayoutDescription() {
            Elements = new[] {
                new ResourceLayoutElementDescription {
                    Name = "Texture Draw Params",
                    Kind = ResourceKind.UniformBuffer,
                    Stages = ShaderStages.Fragment | ShaderStages.Vertex
                }
            }
        });
        TextureDrawParamsUniform = new TypedDeviceBuffer<TextureDrawUniform>(new BufferDescription { Usage = BufferUsage.UniformBuffer }, RenderSystem);
        TextureDrawParamsResourceSet = RenderSystem.ResourceFactory.CreateResourceSet(new ResourceSetDescription {
            BoundResources = new BindableResource[] {
                TextureDrawParamsUniform.BackingBuffer
            },
            Layout = drawUniformLayout
        });

        //Vertex Buffer
        VertexBuffer = RenderSystem.ResourceFactory.CreateBuffer(new BufferDescription {
            SizeInBytes = (uint)Marshal.SizeOf<BasicVertex>() * 4,
            Usage = BufferUsage.VertexBuffer
        });
        RenderSystem.GraphicsDevice.UpdateBuffer(VertexBuffer, 0, new[] {
            new BasicVertex(new vec3(0, 0, 0)),
            new BasicVertex(new vec3(0, 1, 0)),
            new BasicVertex(new vec3(1, 1, 0)),
            new BasicVertex(new vec3(1, 0, 0)),
        });

        //Pipeline
        DrawPipeline = RenderSystem.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription {
            Outputs = nativeAtlasData.Framebuffer.OutputDescription,
            BlendState = BlendStateDescription.SingleOverrideBlend,
            DepthStencilState = new DepthStencilStateDescription {
                DepthWriteEnabled = false,
                DepthTestEnabled = false,
                StencilTestEnabled = false,
            },
            PrimitiveTopology = PrimitiveTopology.TriangleStrip,
            RasterizerState = new RasterizerStateDescription {
                CullMode = FaceCullMode.None,
                DepthClipEnabled = false,
                ScissorTestEnabled = false,
                FillMode = PolygonFillMode.Solid
            },
            ShaderSet = new ShaderSetDescription {
                VertexLayouts = new[] {
                    BasicVertex.Layout
                },
                Shaders = shaders
            },
            ResourceLayouts = new[] {
                RenderSystem.TextureManager.TextureResourceLayout,
                drawUniformLayout,
            }
        });
    }

    private void UnclaimCell(ivec2 pos) {
        if (availableCellsSet.Contains(pos))
            return;

        //Console.Out.WriteLine($"Unclaim Cell {pos}");

        availableCells.Add(pos);
        availableCellsSet.Add(pos);
    }

    public bool IsCellClaimed(ivec2 pos) {
        //Console.Out.WriteLine($"Checking Claimed Cell {pos}");

        return availableCellsSet.Contains(pos);
    }

    public bool TryClaimCell(ivec2 pos) {
        if (!availableCellsSet.Contains(pos)) return false;

        //Console.Out.WriteLine($"Claim Cell {pos}");

        availableCellsSet.Remove(pos);
        availableCells.Remove(pos);
        return true;
    }


    /// <summary>
    /// Stitches a texture into the atlas.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="texture"></param>
    /// <returns></returns>
    public Sprite StitchTexture(string id, Veldrid.Texture texture, ResourceSet resourceSet, ivec2 position, ivec2 size) {

        //If a texture exists at this location, move it.
        if (textures.Remove(id, out var sprite)) {
            foreach (var cell in sprite.Cells)
                UnclaimCell(cell);

            sprite.Cells.Clear();
            sprite.position = ivec2.Zero;
            sprite.size = ivec2.Zero;
        } else {
            sprite = new Sprite(this);
        }

        textures[id] = sprite;

        var cellDimensions = (ivec2)vec2.Ceiling((vec2)size / CellSize);

        //Generate what cells we need for texture.
        for (int x = 0; x < cellDimensions.x; x++)
        for (int y = 0; y < cellDimensions.y; y++)
            sprite.Cells.Add(new ivec2(x, y) * CellSize);

        bool cellFound = false;

        for (int c = 0; c < 4 && cellFound == false; c++) {
            foreach (var availableCell in availableCells) {
                //Check all positions relative to this cell
                //If all needed positions are valid, this cell can be used for this texture.
                bool valid = true;
                foreach (var textureCell in sprite.Cells) {
                    var totalPos = availableCell + textureCell;

                    if (!IsCellClaimed(totalPos)) {
                        valid = false;
                        break;
                    }
                }

                //If not all positions are available, this position isn't valid, so check the next one.
                if (!valid)
                    continue;

                sprite.position = availableCell;
                sprite.size = size;

                //Move all claimed cells to be relative to this object, then claim them.
                for (int i = 0; i < sprite.Cells.Count; i++) {
                    sprite.Cells[i] += sprite.position;
                    TryClaimCell(sprite.Cells[i]);
                }

                //Valid cell was found, break out of loop.
                cellFound = true;
                break;
            }

            if (!cellFound) {
                DoubleSize();
            }
        }

        if (!cellFound)
            throw new Exception($"Unable to find space in atlas for sprite {id}, size is {size}, atlas size is {this.size}");
        
        //Update uniform...
        var uniformData = TextureDrawParamsUniform.value;
        //Source...
        uniformData.SrcMin = position;
        uniformData.SrcMax = position + size;
        //Destination...
        uniformData.DstMin = sprite.position;
        uniformData.DstMax = uniformData.DstMin + sprite.size;

        uniformData.SrcSize = new vec2(texture.Width, texture.Height);
        uniformData.DstSize = this.size;

        //Blit from texture to framebuffer.
        Blit(uniformData, resourceSet, nativeAtlasData.Framebuffer);

        //Return the texture we just got.
        return sprite;
    }

    public bool TryGetSprite(string id, [NotNullWhen(true)] out Sprite? texture) => textures.TryGetValue(id, out texture);

    public void DoubleSize() {
        Game.Logger.Info("Doubling atlas size...");

        //Copy Texture
        var newWidth = nativeAtlasData.Width * 2;
        var newHeight = nativeAtlasData.Height * 2;

        var newAtlasData = new NativeAtlasData(newWidth, newHeight, RenderSystem);
        var currentAtlasData = nativeAtlasData;

        var data = new TextureDrawUniform {
            SrcMin = vec2.Zero,
            SrcMax = currentAtlasData.Size,
            SrcSize = currentAtlasData.Size,
            DstMin = vec2.Zero,
            DstMax = currentAtlasData.Size,
            DstSize = newAtlasData.Size
        };

        Blit(data, currentAtlasData.ResourceSet, newAtlasData.Framebuffer);

        //Insert new cells
        var oldCellLimit = new ivec2(currentAtlasData.Width - CellSize, currentAtlasData.Height - CellSize);
        for (int x = 0; x < newWidth; x += CellSize)
        for (int y = 0; y < newHeight; y += CellSize) {
            var cellPos = new ivec2(x, y);

            if ((cellPos <= oldCellLimit).All)
                continue;

            UnclaimCell(cellPos);
        }

        nativeAtlasData = newAtlasData;
        currentAtlasData.Dispose();
    }

    private void Blit(TextureDrawUniform drawUniform, ResourceSet source, Framebuffer destination) {
        //Upload uniform to GPU.
        TextureDrawParamsUniform.SetValue(drawUniform, CommandList);

        CommandList.SetPipeline(DrawPipeline);
        CommandList.SetFramebuffer(destination);

        //Set resource sets...
        CommandList.SetGraphicsResourceSet(0, source);
        CommandList.SetGraphicsResourceSet(1, TextureDrawParamsResourceSet);

        //Finally, draw a quad at the desired location.
        CommandList.SetVertexBuffer(0, VertexBuffer);
        CommandList.SetIndexBuffer(RenderSystem.CommonIndexBuffer, IndexFormat.UInt32);
        CommandList.DrawIndexed(6);
    }

    /// <summary>
    /// Represents an atlas that's contained within a texture.
    /// </summary>
    public class Sprite {
        public readonly Atlas Atlas;

        public readonly List<ivec2> Cells = new();

        public ivec2 position;
        public ivec2 size;

        public vec2 uvPosition => (vec2)position / Atlas.size;
        public vec2 uvSize => (vec2)size / Atlas.size;

        public Sprite(Atlas atlas) {
            this.Atlas = atlas;
        }

        //Glorified component-wise lerp function
        public vec2 GetTrueUV(vec2 baseUV) {
            //Helps fix seams between sprite textures in the atlas.
            baseUV -= 0.5f;
            baseUV *= 0.99f;
            baseUV += 0.5f;
            
            baseUV = new vec2(baseUV.x, baseUV.y);
            return uvPosition + (uvSize * baseUV);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct TextureDrawUniform {
        public const uint Size = sizeof(float) * 2 * 6;

        public vec2 SrcMin;
        public vec2 SrcMax;
        public vec2 DstMin;
        public vec2 DstMax;
        public vec2 SrcSize;
        public vec2 DstSize;
    }


    private class NativeAtlasData : IDisposable {

        private readonly RenderSystem RenderSystem;

        /// <summary>
        /// The actual device texture of this atlas.
        /// </summary>
        public readonly Veldrid.Texture Texture;

        /// <summary>
        /// The framebuffer we can use to draw to the atlas's texture.
        /// </summary>
        public readonly Framebuffer Framebuffer;

        /// <summary>
        /// The resource set we can use to bind the atlas's texture.
        /// </summary>
        public readonly ResourceSet ResourceSet;

        public readonly int Width, Height;

        public ivec2 Size => new(Width, Height);

        public NativeAtlasData(int width, int height, RenderSystem renderSystem) {
            Width = width;
            Height = height;
            RenderSystem = renderSystem;


            // Atlas textures need to be read as an actual texture,
            // Have mipmaps,
            // AND be used as a render target so we can write to them.
            Texture = renderSystem.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
                (uint)Width, (uint)Height,
                4, 1,
                PixelFormat.R32_G32_B32_A32_Float,
                TextureUsage.Sampled | TextureUsage.RenderTarget | TextureUsage.GenerateMipmaps
            ));

            Framebuffer = renderSystem.ResourceFactory.CreateFramebuffer(new FramebufferDescription {
                ColorTargets = new[] {
                    new FramebufferAttachmentDescription {
                        Target = Texture
                    }
                }
            });

            ResourceSet = renderSystem.TextureManager.CreateTextureResourceSet(Texture);
        }

        public void GenerateMipMaps(RenderSystem system) => system.MainCommandList.GenerateMipmaps(Texture);

        public void Dispose() {
            RenderSystem.Dispose(Framebuffer);
            RenderSystem.Dispose(ResourceSet);
            RenderSystem.Dispose(Texture);
        }
    }

    public void GenerateMipmaps() => nativeAtlasData.GenerateMipMaps(RenderSystem);
}
