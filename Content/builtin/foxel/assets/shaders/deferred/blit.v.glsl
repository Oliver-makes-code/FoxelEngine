#version 440

#include "deferred/common.glsl"

USER_LAYOUT(0, 0) uniform sampler SsaoTextureSampler;
USER_LAYOUT(0, 1) uniform texture2D SsaoTexture;

#ifdef FRAGMENT

#define USE_DEBUG_QUAD_SCREEN

void frag() {
    #ifdef USE_DEBUG_QUAD_SCREEN
    vec2 uv = fs_Uv * 2;
    o_Color = vec4(uv, 1, 1);
    if (uv.x < 1 && uv.y < 1) {
        o_Color = gSample(TEXTURE_COLOR, uv);
    } else if (uv.x < 1) {
        vec4 color = gSample(TEXTURE_NORMAL, uv - vec2(0, 1));
        o_Color = vec4((color.rgb + 1) * 0.5, color.a);
    } else if (uv.y < 1) {
        o_Color = gSample(TEXTURE_POSITION, uv - vec2(1, 0));
    } else {
        float depth = texture(sampler2D(SsaoTexture, SsaoTextureSampler), uv - vec2(1, 1)).r;
        o_Color = vec4(depth, depth, depth, 1);
    }
    #else
    o_Color = gSample(TEXTURE_COLOR, fs_Uv) * texture(sampler2D(SsaoTexture, SsaoTextureSampler), fs_Uv).r;
    #endif
}

#endif
