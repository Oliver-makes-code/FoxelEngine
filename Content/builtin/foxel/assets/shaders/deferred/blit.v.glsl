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
    o_Color = texture(sampler2D(ColorTexture, ColorTextureSampler), fs_Uv);
}

#endif
