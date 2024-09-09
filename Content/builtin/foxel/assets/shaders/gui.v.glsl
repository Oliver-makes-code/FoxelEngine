#version 440

#include "foxel:common/filtering.glsl"

layout (set = 0, binding = 0) uniform ScreenData {
    vec2 ScreenSize;
    vec2 InverseScreenSize;
};

layout (set = 0, binding = 1) uniform GuiData {
    int GuiScale;
};

layout (set = 1, binding = 0) uniform sampler TextureSampler;
layout (set = 1, binding = 1) uniform texture2D Texture;

vert_param(0, vec2 vs_I_Position)
vert_param(1, vec2 vs_ScreenAnchor)
vert_param(2, vec2 vs_TextureAnchor)
vert_param(3, ivec2 vs_Position)
vert_param(4, ivec2 vs_Size)
vert_param(5, vec4 vs_Color)
vert_param(6, vec2 vs_UvMin)
vert_param(7, vec2 vs_UvMax)
vert_param(8, vec3 vs_Normal)
frag_param(0, vec4 fs_Color)
frag_param(1, vec2 fs_Uv)
frag_param(2, vec2 fs_UvMin)
frag_param(3, vec2 fs_UvMax)
out_param(0, vec4 o_Color)

#ifdef VERTEX

void vert() {
    vec2 flip = vec2(-1, 1);
    vec2 pos = vs_I_Position;
    pos += vs_TextureAnchor * flip;
    pos *= vs_Size * GuiScale;
    pos -= vs_ScreenAnchor * ScreenSize * flip;
    pos -= vs_Position * GuiScale * 2 * flip;
    gl_Position = vec4(pos * InverseScreenSize, 0, 1);
    gl_Position.y *= -1;
    fs_Color = vs_Color;
    int yIdx = gl_VertexIndex >> 1;
    int xIdx = (gl_VertexIndex & 1) ^ yIdx;
    float[] x = { vs_UvMin.x, vs_UvMax.x };
    float[] y = { vs_UvMax.y, vs_UvMin.y };
    fs_Uv = vec2(x[xIdx], y[yIdx]);
    fs_UvMax = vs_UvMax;
    fs_UvMin = vs_UvMin;
}

#endif
#ifdef FRAGMENT

void frag() {
    if (fs_Uv.x < 0 || fs_Uv.y < 0) {
        o_Color = fs_Color;
        return;
    }
    vec4 sampledColor = colorBlendAverage(interpolatePixels(fs_Uv, fs_UvMin, fs_UvMax, Texture, TextureSampler));
    o_Color = vec4(colorBlendUniform(sampledColor.rgb, sampledColor.rgb * fs_Color.rgb, 0.15), sampledColor.a * fs_Color.a);
}

#endif
