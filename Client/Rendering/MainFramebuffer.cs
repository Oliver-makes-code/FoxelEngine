using System;
using System.Collections.Generic;
using Veldrid;
using Foxel.Core.Rendering;

namespace Foxel.Client.Rendering;

public class MainFramebuffer : IDisposable {
    public readonly Framebuffer Framebuffer;
    public readonly Framebuffer WindowFramebuffer;

    public readonly Veldrid.Texture MainColor;
    public readonly Veldrid.Texture Staging;
    public readonly Veldrid.Texture Normal;
    public readonly Veldrid.Texture Depth;

    public readonly TextureSampleCount Samples;

    public readonly Veldrid.Texture ResolvedMainColor;
    public readonly ResourceSet ResolvedMainColorSet;
    public readonly Veldrid.Texture ResolvedNormal;
    public readonly ResourceSet ResolvedNormalSet;
    public readonly Veldrid.Texture ResolvedDepth;


    public readonly List<IDisposable> Dependencies = new();

    public MainFramebuffer(TextureManager textureManager, ResourceFactory factory, Framebuffer windowBuffer, uint width, uint height, uint sampleCount = 1) {
        switch (sampleCount) {
            default:
                Samples = TextureSampleCount.Count1;
                break;
            case 2:
                Samples = TextureSampleCount.Count2;
                break;
            case 4:
                Samples = TextureSampleCount.Count4;
                break;
            case 8:
                Samples = TextureSampleCount.Count8;
                break;
        }

        var baseDescription = new TextureDescription {
            Width = width,
            Height = height,
            Depth = 1,
            ArrayLayers = 1,
            MipLevels = 1,
            Type = TextureType.Texture2D,
            SampleCount = Samples,
            Format = PixelFormat.R16_G16_B16_A16_Float,
            Usage = TextureUsage.RenderTarget | TextureUsage.Sampled
        };

        MainColor = AddDependency(factory.CreateTexture(baseDescription));
        Normal = AddDependency(factory.CreateTexture(baseDescription));

        baseDescription.Format = PixelFormat.D32_Float_S8_UInt;
        baseDescription.Usage = TextureUsage.DepthStencil | TextureUsage.Sampled;

        Depth = AddDependency(factory.CreateTexture(baseDescription));

        if (Samples == TextureSampleCount.Count1) {
            ResolvedMainColor = MainColor;
            ResolvedNormal = Normal;
            ResolvedDepth = Depth;
        } else {
            baseDescription.SampleCount = TextureSampleCount.Count1;

            baseDescription.Format = PixelFormat.R16_G16_B16_A16_Float;
            baseDescription.Usage = TextureUsage.RenderTarget | TextureUsage.Sampled;

            ResolvedMainColor = AddDependency(factory.CreateTexture(baseDescription));
            ResolvedNormal = AddDependency(factory.CreateTexture(baseDescription));

            baseDescription.Format = PixelFormat.D32_Float_S8_UInt;
            baseDescription.Usage = TextureUsage.DepthStencil | TextureUsage.Sampled;

            ResolvedDepth = AddDependency(factory.CreateTexture(baseDescription));
        }

        ResolvedMainColorSet = textureManager.CreateTextureResourceSet(ResolvedMainColor);
        ResolvedNormalSet = textureManager.CreateTextureResourceSet(ResolvedNormal);

        baseDescription.Format = PixelFormat.R16_G16_B16_A16_Float;
        baseDescription.SampleCount = TextureSampleCount.Count1;
        baseDescription.Usage = TextureUsage.Staging;

        Staging = AddDependency(factory.CreateTexture(baseDescription));

        Framebuffer = AddDependency(factory.CreateFramebuffer(new FramebufferDescription {
            ColorTargets = [
                new FramebufferAttachmentDescription(MainColor, 0),
                new FramebufferAttachmentDescription(Normal, 0),
            ],
            DepthTarget = new FramebufferAttachmentDescription(Depth, 0)
        }));
        WindowFramebuffer = windowBuffer;
    }

    public void Resolve(RenderSystem renderSystem) {
        //If MSAA is 1, there's no need to resolve.
        if (Samples == TextureSampleCount.Count1) return;

        renderSystem.MainCommandList.ResolveTexture(MainColor, ResolvedMainColor);
        renderSystem.MainCommandList.ResolveTexture(Normal, ResolvedNormal);
        //renderSystem.MainCommandList.ResolveTexture(Depth, ResolvedDepth);
    }


    public T AddDependency<T>(T toAdd) where T : IDisposable {
        Dependencies.Add(toAdd);
        return toAdd;
    }

    public void Dispose() {
        foreach (var dependency in Dependencies)
            dependency.Dispose();
    }
}
