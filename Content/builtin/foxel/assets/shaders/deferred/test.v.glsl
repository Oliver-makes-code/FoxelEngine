#version 440

#include "deferred/common.glsl"

#ifdef FRAGMENT

void frag() {
    o_Color = texture(sampler2D(ColorTexture, ColorTextureSampler), fs_Uv);
}

#endif
