#version 440

#include "deferred/common.glsl"

layout (set = 5, binding = 0) uniform TextureDrawParams {
    bool flip;
};

vert_param(0, vec2 vs_Position)
frag_param(0, vec2 fs_Uv)
out_param(0, vec4 o_Color)

#ifdef VERTEX

void vert() {
    gl_Position = vec4((vs_Position.xy * 2) - 1, 0, 1);

    //Sample from source texture.

    fs_Uv = vs_Position.xy;
    if (flip)
        fs_Uv = vec2(fs_Uv.x, 1 - fs_Uv.y);
}

#endif
#ifdef FRAGMENT

void frag() {
    #ifdef BLIT_USE_QUAD_SCREEN
    vec2 uv = fs_Uv * 2;
    if (uv.x < 1 && uv.y < 1) {
        o_Color = texture(sampler2D(ColorTexture, ColorTextureSampler), uv);
    } else if (uv.x < 1) {
        vec4 color = texture(sampler2D(NormalTexture, NormalTextureSampler), uv - vec2(0, 1));
        o_Color = vec4((color.rgb + 1) * 0.5, color.a);
    } else if (uv.y < 1) {
        o_Color = texture(sampler2D(PositionTexture, PositionTextureSampler), uv - vec2(1, 0));
    } else {
        float depth = texture(sampler2D(DepthTexture, DepthTextureSampler), uv - vec2(1, 1)).r;
        o_Color = vec4(depth, depth, depth, 1);
    }
    #else
    o_Color = texture(sampler2D(ColorTexture, ColorTextureSampler), fs_Uv);
    #endif
}

#endif
