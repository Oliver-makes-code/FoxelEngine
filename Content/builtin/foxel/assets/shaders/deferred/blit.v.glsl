#version 440

#include "deferred/common.glsl"

USER_LAYOUT(0, 0) uniform sampler StageTextureSampler;
USER_LAYOUT(0, 1) uniform texture2D StageTexture;

#ifdef FRAGMENT

#define USE_DEBUG_QUAD_SCREEN

void frag() {
    #ifdef USE_DEBUG_QUAD_SCREEN
    vec2 uv = fs_Uv * 2;
    o_Color = vec4(uv, 1, 1);
    if (uv.x < 1 && uv.y < 1) {
        o_Color = texture(sampler2D(Texture[TEXTURE_COLOR], TextureSampler), uv);
    } else if (uv.x < 1) {
        vec4 color = texture(sampler2D(Texture[TEXTURE_NORMAL], TextureSampler), uv - vec2(0, 1));
        o_Color = vec4((color.rgb + 1) * 0.5, color.a);
    } else if (uv.y < 1) {
        o_Color = texture(sampler2D(Texture[TEXTURE_POSITION], TextureSampler), uv - vec2(1, 0));
    } else {
        float depth = texture(sampler2D(Texture[TEXTURE_DEPTH], TextureSampler), uv - vec2(1, 1)).r;
        o_Color = vec4(depth, depth, depth, 1);
    }
    #else
    o_Color = texture(sampler2D(Texture[TEXTURE_COLOR], TextureSampler), fs_Uv);
    #endif
}

#endif
