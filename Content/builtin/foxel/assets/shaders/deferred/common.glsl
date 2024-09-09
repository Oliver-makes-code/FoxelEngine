layout (set = 0, binding = 0) uniform sampler ColorTextureSampler;
layout (set = 0, binding = 1) uniform texture2D ColorTexture;

layout (set = 1, binding = 0) uniform sampler NormalTextureSampler;
layout (set = 1, binding = 1) uniform texture2D NormalTexture;

layout (set = 2, binding = 0) uniform sampler PositionTextureSampler;
layout (set = 2, binding = 1) uniform texture2D PositionTexture;

layout (set = 3, binding = 0) uniform sampler DepthTextureSampler;
layout (set = 3, binding = 1) uniform texture2D DepthTexture;

layout (set = 4, binding = 0) uniform ScreenData {
    vec2 ScreenSize;
    vec2 InverseScreenSize;
};

vert_param(0, vec2 vs_Position)
frag_param(0, vec2 fs_Uv)
out_param(0, vec4 o_Color)

#ifndef VERTEX_HANDLED
#ifdef VERTEX

void vert() {
    gl_Position = vec4((vs_Position.xy * 2) - 1, 0, 1);

    //Sample from source texture.

    fs_Uv = vs_Position.xy;
    fs_Uv.y *= -1;
}

#endif
#endif
