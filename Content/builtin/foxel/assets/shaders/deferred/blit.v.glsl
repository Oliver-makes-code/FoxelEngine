#version 440

#include "deferred/common.glsl"

layout (set = 5, binding = 0) uniform sampler StageTextureSampler;
layout (set = 5, binding = 1) uniform texture2D StageTexture;

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
    o_Color = texture(sampler2D(StageTexture, StageTextureSampler), fs_Uv);
    #endif
}

#endif
